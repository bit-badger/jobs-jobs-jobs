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


open BitBadger.Npgsql.FSharp.Documents
open Npgsql.FSharp

/// Connection management for the document store
[<AutoOpen>]
module DataConnection =
    
    open Microsoft.Extensions.Configuration
    open Npgsql
    
    /// Get the data source as the start of a SQL statement
    let dataSource =
        Configuration.dataSource >> Sql.fromDataSource
    
    /// Create tables
    let private createTables () = backgroundTask {
        let! _ =
            dataSource ()
            |> Sql.query "CREATE SCHEMA IF NOT EXISTS jjj"
            |> Sql.executeNonQueryAsync
        do! Definition.ensureTable Table.Citizen
        do! Definition.ensureTable Table.Continent
        do! Definition.ensureTable Table.Listing
        do! Definition.ensureTable Table.Success
        let sql = [
            // Tables that use more than the default document configuration
            $"CREATE TABLE IF NOT EXISTS {Table.Profile}      (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL,
                                                               text_search TSVECTOR NOT NULL,
                CONSTRAINT fk_profile_citizen       FOREIGN KEY (id) REFERENCES {Table.Citizen} (id) ON DELETE CASCADE)"
            $"CREATE TABLE IF NOT EXISTS {Table.SecurityInfo} (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL,
                CONSTRAINT fk_security_info_citizen FOREIGN KEY (id) REFERENCES {Table.Citizen} (id) ON DELETE CASCADE)"
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
        Configuration.useDataSource (builder.Build ())
        do! createTables ()
        do! createTriggers ()
    }


/// Create a match-anywhere clause for a LIKE or ILIKE clause
let like value =
    Sql.string $"%%%s{value}%%"

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
        Custom.list $"{Query.selectFromTable Table.Continent} ORDER BY data ->> 'name'" [] fromData<Continent>
    
    /// Retrieve a continent by its ID
    let findById continentId =
        Find.byId<Continent> Table.Continent (ContinentId.toString continentId)
