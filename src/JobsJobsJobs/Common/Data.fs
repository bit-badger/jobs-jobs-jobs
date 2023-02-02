module JobsJobsJobs.Common.Data

/// Constants for tables used by Jobs, Jobs, Jobs
[<RequireQualifiedAccess>]
module Table =
    
    /// Citizens
    [<Literal>]
    let Citizen = "jjj.citizen"
    
    /// Continents
    [<Literal>]
    let Continent = "jjj.continent"
    
    /// Job Listings
    [<Literal>]
    let Listing = "jjj.listing"
    
    /// Employment Profiles
    [<Literal>]
    let Profile = "jjj.profile"
    
    /// User Security Information
    [<Literal>]
    let SecurityInfo = "jjj.security_info"
    
    /// Success Stories
    [<Literal>]
    let Success = "jjj.success"


open Npgsql.FSharp

/// Connection management for the document store
[<AutoOpen>]
module DataConnection =
    
    open Microsoft.Extensions.Configuration
    open Npgsql
    
    /// The data source for the document store
    let mutable private theDataSource : NpgsqlDataSource option = None
    
    /// Get the data source as the start of a SQL statement
    let dataSource () =
        match theDataSource with
        | Some ds -> Sql.fromDataSource ds
        | None -> invalidOp "DataConnection.setUp() must be called before accessing the database"
    
    /// Create tables
    let private createTables () = backgroundTask {
        let sql = [
            "CREATE SCHEMA IF NOT EXISTS jjj"
            // Tables
            $"CREATE TABLE IF NOT EXISTS {Table.Citizen}      (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL)"
            $"CREATE TABLE IF NOT EXISTS {Table.Continent}    (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL)"
            $"CREATE TABLE IF NOT EXISTS {Table.Listing}      (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL)"
            $"CREATE TABLE IF NOT EXISTS {Table.Profile}      (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL,
                                                               text_search TSVECTOR NOT NULL,
                CONSTRAINT fk_profile_citizen       FOREIGN KEY (id) REFERENCES {Table.Citizen} (id) ON DELETE CASCADE)"
            $"CREATE TABLE IF NOT EXISTS {Table.SecurityInfo} (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL,
                CONSTRAINT fk_security_info_citizen FOREIGN KEY (id) REFERENCES {Table.Citizen} (id) ON DELETE CASCADE)"
            $"CREATE TABLE IF NOT EXISTS {Table.Success}      (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL)"
            // Key indexes
            $"CREATE UNIQUE INDEX IF NOT EXISTS uk_citizen_email      ON {Table.Citizen} ((data -> 'email'))"
            $"CREATE        INDEX IF NOT EXISTS idx_listing_citizen   ON {Table.Listing} ((data -> 'citizenId'))"
            $"CREATE        INDEX IF NOT EXISTS idx_listing_continent ON {Table.Listing} ((data -> 'continentId'))"
            $"CREATE        INDEX IF NOT EXISTS idx_profile_continent ON {Table.Profile} ((data -> 'continentId'))"
            $"CREATE        INDEX IF NOT EXISTS idx_success_citizen   ON {Table.Success} ((data -> 'citizenId'))"
            // Profile text search index
            $"CREATE INDEX IF NOT EXISTS idx_profile_search ON {Table.Profile} USING GIN(text_search)"
        ]
        let! _ =
            dataSource ()
            |> Sql.executeTransactionAsync (sql |> List.map (fun sql -> sql, [ [] ]))
        ()
    }

    /// Create functions and triggers required to 
    let createTriggers () = backgroundTask {
        let! functions =
            dataSource ()
            |> Sql.query
                "SELECT p.proname
                   FROM pg_catalog.pg_proc p
                        LEFT JOIN pg_catalog.pg_namespace n ON n.oid = p.pronamespace
                  WHERE n.nspname = 'jjj'"
            |> Sql.executeAsync (fun row -> row.string "proname")
        if not (functions |> List.contains "indexable_array_string") then
            let! _ =
                dataSource ()
                |> Sql.query """
                    CREATE FUNCTION jjj.indexable_array_string(target jsonb, path jsonpath) RETURNS text AS $$
                        BEGIN
                            RETURN REPLACE(REPLACE(REPLACE(REPLACE(jsonb_path_query_array(target, path)::text,
                                    '["', ''), '", "', ' '), '"]', ''), '[]', '');
                        END;
                    $$ LANGUAGE plpgsql;"""
                |> Sql.executeNonQueryAsync
            ()
        if not (functions |> List.contains "set_text_search") then
            let! _ =
                dataSource ()
                |> Sql.query $"
                    CREATE FUNCTION jjj.set_text_search() RETURNS trigger AS $$
                        BEGIN
                            NEW.text_search := to_tsvector('english',
                                   COALESCE(NEW.data ->> 'region',     '') || ' '
                                || COALESCE(NEW.data ->> 'biography',  '') || ' '
                                || COALESCE(NEW.data ->> 'experience', '') || ' '
                                || jjj.indexable_array_string(NEW.data, '$.skills[*].description') || ' '
                                || jjj.indexable_array_string(NEW.data, '$.history[*].employer')   || ' '
                                || jjj.indexable_array_string(NEW.data, '$.history[*].position')   || ' '
                                || jjj.indexable_array_string(NEW.data, '$.history[*].description'));
                            RETURN NEW;
                        END;
                    $$ LANGUAGE plpgsql;
                    CREATE TRIGGER set_text_search BEFORE INSERT OR UPDATE ON {Table.Profile}
                        FOR EACH ROW EXECUTE FUNCTION jjj.set_text_search();"
                |> Sql.executeNonQueryAsync
            ()
    }
    
    /// Set up the data connection from the given configuration
    let setUp (cfg : IConfiguration) = backgroundTask {
        let builder = NpgsqlDataSourceBuilder (cfg.GetConnectionString "PostgreSQL")
        let _ = builder.UseNodaTime ()
        theDataSource <- Some (builder.Build ())
        do! createTables ()
        do! createTriggers ()
    }


open System.Text.Json
open System.Threading.Tasks
open JobsJobsJobs

/// Map the data field to the requested document type
let toDocumentFrom<'T> fieldName (row : RowReader) =
    JsonSerializer.Deserialize<'T> (row.string fieldName, Json.options)

/// Map the data field to the requested document type
let toDocument<'T> (row : RowReader) = toDocumentFrom<'T> "data" row

/// Get a document
let getDocument<'T> table docId sqlProps : Task<'T option> = backgroundTask {
    let! doc =
        Sql.query $"SELECT * FROM %s{table} where id = @id" sqlProps
        |> Sql.parameters [ "@id", Sql.string docId ]
        |> Sql.executeAsync toDocument
    return List.tryHead doc
}

/// Serialize a document to JSON
let mkDoc<'T> (doc : 'T) =
    JsonSerializer.Serialize<'T> (doc, Json.options)
    
/// Save a document
let saveDocument table docId sqlProps doc = backgroundTask {
    let! _ =
        Sql.query
            $"INSERT INTO %s{table} (id, data) VALUES (@id, @data)
                ON CONFLICT (id) DO UPDATE SET data = EXCLUDED.data"
            sqlProps
        |> Sql.parameters
            [   "@id",   Sql.string docId
                "@data", Sql.jsonb  doc ]
        |> Sql.executeNonQueryAsync
    ()
}

/// Create a match-anywhere clause for a LIKE or ILIKE clause
let like value =
    Sql.string $"%%%s{value}%%"

/// The JSON access operator ->> makes values text; this makes a parameter that will compare the properly
let jsonBool value =
    Sql.string (if value then "true" else "false")

/// Get the SQL for a search WHERE clause
let searchSql criteria =
    let sql = criteria |> List.map fst |> String.concat " AND "
    if sql = "" then "" else $"AND {sql}"


/// Continent data access functions
[<RequireQualifiedAccess>]
module Continents =
    
    open JobsJobsJobs.Domain

    /// Retrieve all continents
    let all () =
        dataSource ()
        |> Sql.query $"SELECT * FROM {Table.Continent} ORDER BY data ->> 'name'"
        |> Sql.executeAsync toDocument<Continent>
    
    /// Retrieve a continent by its ID
    let findById continentId =
        dataSource () |> getDocument<Continent> Table.Continent (ContinentId.toString continentId)
