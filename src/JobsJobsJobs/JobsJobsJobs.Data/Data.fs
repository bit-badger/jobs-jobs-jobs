namespace JobsJobsJobs.Data

/// Constants for tables used by Jobs, Jobs, Jobs
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

/// Connection management for the Marten document store
module DataConnection =
    
    open Microsoft.Extensions.Configuration
    
    /// The configuration from which a document store will be created
    let mutable private config : IConfiguration option = None
    
    /// Get the connection string
    let connection () =
        match config with
        | Some cfg -> Sql.connect (cfg.GetConnectionString "PostgreSQL")
        | None -> invalidOp "Connection.setUp() must be called before accessing the database"
    
    /// Create tables
    let private createTables () = backgroundTask {
        let sql = [
            $"CREATE SCHEMA IF NOT EXISTS jjj"
            $"CREATE TABLE IF NOT EXISTS {Table.Citizen} (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL)"
            $"CREATE TABLE IF NOT EXISTS {Table.Continent} (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL)"
            $"CREATE TABLE IF NOT EXISTS {Table.Listing} (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL)"
            $"CREATE TABLE IF NOT EXISTS {Table.Profile} (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL,
                CONSTRAINT fk_profile_citizen FOREIGN KEY (id) REFERENCES {Table.Citizen} (id) ON DELETE CASCADE)"
            $"CREATE TABLE IF NOT EXISTS {Table.SecurityInfo} (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL,
                CONSTRAINT fk_security_info_citizen FOREIGN KEY (id) REFERENCES {Table.Citizen} (id) ON DELETE CASCADE)"
            $"CREATE TABLE IF NOT EXISTS {Table.Success} (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL)"
            $"CREATE INDEX IF NOT EXISTS idx_citizen_email     ON {Table.Citizen} USING GIN ((data -> 'email'))"
            $"CREATE INDEX IF NOT EXISTS idx_listing_citizen   ON {Table.Listing} USING GIN ((data -> 'citizenId'))"
            $"CREATE INDEX IF NOT EXISTS idx_listing_continent ON {Table.Listing} USING GIN ((data -> 'continentId'))"
            $"CREATE INDEX IF NOT EXISTS idx_profile_continent ON {Table.Profile} USING GIN ((data -> 'continentId'))"
            $"CREATE INDEX IF NOT EXISTS idx_success_citizen   ON {Table.Success} USING GIN ((data -> 'citizenId'))"
        ]
        let! _ =
            connection ()
            |> Sql.executeTransactionAsync (sql |> List.map (fun sql -> sql, [ [] ]))
        ()
    }
    
    /// Set up the data connection from the given configuration
    let setUp (cfg : IConfiguration) = backgroundTask {
        config <- Some cfg
        do! createTables ()
    }


open DataConnection

/// Helper functions for data manipulation
[<AutoOpen>]
module private Helpers =
    
    open System.Text.Json
    open System.Threading.Tasks
    
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
                    [ "@id",   Sql.string docId
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


open JobsJobsJobs.Domain

/// Citizen data access functions
[<RequireQualifiedAccess>]
module Citizens =

    open Npgsql
    
    /// Delete a citizen by their ID
    let deleteById citizenId = backgroundTask {
        let! _ =
            connection ()
            |> Sql.query $"
                DELETE FROM {Table.Success} WHERE data ->> 'citizenId' = @id;
                DELETE FROM {Table.Listing} WHERE data ->> 'citizenId' = @id;
                DELETE FROM {Table.Citizen} WHERE id                   = @id"
            |> Sql.parameters [ "@id", Sql.string (CitizenId.toString citizenId) ]
            |> Sql.executeNonQueryAsync
        ()
    }
    
    /// Find a citizen by their ID
    let findById citizenId = backgroundTask {
        match! connection () |> getDocument<Citizen> Table.Citizen (CitizenId.toString citizenId) with
        | Some c when not c.IsLegacy -> return Some c
        | Some _
        | None -> return None
    }
    
    /// Save a citizen
    let private saveCitizen (citizen : Citizen) connProps =
        saveDocument Table.Citizen (CitizenId.toString citizen.Id) connProps (mkDoc citizen) 
    
    /// Save security information for a citizen
    let private saveSecurity (security : SecurityInfo) connProps =
        saveDocument Table.SecurityInfo (CitizenId.toString security.Id) connProps (mkDoc security)
    
    /// Save a citizen
    let save citizen =
        saveCitizen citizen (connection ())
        
    /// Register a citizen (saves citizen and security settings)
    let register citizen (security : SecurityInfo) = backgroundTask {
        let connProps = connection ()
        use conn      = Sql.createConnection connProps
        do! conn.OpenAsync ()
        use! txn = conn.BeginTransactionAsync ()
        do! saveCitizen  citizen  connProps
        do! saveSecurity security connProps
        do! txn.CommitAsync ()
    }
    
    /// Attempt a user log on
    let tryLogOn email (pwCheck : string -> bool) now = backgroundTask {
        let  connProps  = connection ()
        let! tryCitizen =
            connProps
            |> Sql.query $"
                SELECT *
                  FROM {Table.Citizen}
                 WHERE data ->> 'email'    = @email
                   AND data ->> 'isLegacy' = 'false'"
            |> Sql.parameters [ "@email", Sql.string email ]
            |> Sql.executeAsync toDocument<Citizen>
        match List.tryHead tryCitizen with
        | Some citizen ->
            let citizenId = CitizenId.toString citizen.Id
            let! tryInfo = getDocument<SecurityInfo> Table.SecurityInfo citizenId connProps
            let! info = backgroundTask {
                match tryInfo with
                | Some it -> return it
                | None ->
                    let it = { SecurityInfo.empty with Id = citizen.Id }
                    do! saveSecurity it connProps
                    return it
            }
            if info.AccountLocked then return Error "Log on unsuccessful (Account Locked)"
            elif pwCheck citizen.PasswordHash then
                do! saveSecurity { info    with FailedLogOnAttempts = 0   } connProps
                do! saveCitizen  { citizen with LastSeenOn          = now } connProps
                return Ok { citizen with LastSeenOn = now }
            else
                let locked = info.FailedLogOnAttempts >= 4
                do! { info with FailedLogOnAttempts = info.FailedLogOnAttempts + 1; AccountLocked = locked }
                    |> saveSecurity <| connProps
                return Error $"""Log on unsuccessful{if locked then " - Account is now locked" else ""}"""
        | None -> return Error "Log on unsuccessful"
    }


/// Continent data access functions
[<RequireQualifiedAccess>]
module Continents =
    
    /// Retrieve all continents
    let all () =
        connection ()
        |> Sql.query $"SELECT * FROM {Table.Continent}"
        |> Sql.executeAsync toDocument<Continent>
    
    /// Retrieve a continent by its ID
    let findById continentId =
        connection () |> getDocument<Continent> Table.Continent (ContinentId.toString continentId)


open JobsJobsJobs.Domain.SharedTypes

/// Job listing access functions
[<RequireQualifiedAccess>]
module Listings =
    
    /// The SQL to select a listing view
    let viewSql =
        $"SELECT l.*, c.data AS cont_data
            FROM {Table.Listing} l
                 INNER JOIN {Table.Continent} c ON c.id = l.data ->> 'continentId'"
    
    /// Map a result for a listing view
    let private toListingForView row =
        { listing = toDocument<Listing> row; continent = toDocumentFrom<Continent> "cont_data" row }
    
    /// Find all job listings posted by the given citizen
    let findByCitizen citizenId =
        connection ()
        |> Sql.query $"{viewSql} WHERE l.data ->> 'citizenId' = @citizenId AND l.data ->> 'isLegacy' = 'false'"
        |> Sql.parameters [ "@citizenId", Sql.string (CitizenId.toString citizenId) ]
        |> Sql.executeAsync toListingForView
    
    /// Find a listing by its ID
    let findById listingId = backgroundTask {
        match! connection () |> getDocument<Listing> Table.Listing (ListingId.toString listingId) with
        | Some listing when not listing.IsLegacy -> return Some listing
        | Some _
        | None -> return None
    }
    
    /// Find a listing by its ID for viewing (includes continent information)
    let findByIdForView listingId = backgroundTask {
        let! tryListing =
            connection ()
            |> Sql.query $"{viewSql} WHERE id = @id AND l.data ->> 'isLegacy' = 'false'"
            |> Sql.parameters [ "@id", Sql.string (ListingId.toString listingId) ]
            |> Sql.executeAsync toListingForView
        return List.tryHead tryListing
    }
    
    /// Save a listing
    let save (listing : Listing) =
        connection () |> saveDocument Table.Listing (ListingId.toString listing.Id) <| mkDoc listing
    
    /// Search job listings
    let search (search : ListingSearch) =
        let searches = [
            match search.continentId with
            | Some contId -> "l.data ->> 'continentId' = @continentId", [ "@continentId", Sql.string contId ]
            | None -> ()
            match search.region with
            | Some region -> "l.data ->> 'region' ILIKE @region", [ "@region", like region ]
            | None -> ()
            if search.remoteWork <> "" then
                "l.data ->> 'isRemote' = @remote", [ "@remote", jsonBool (search.remoteWork = "yes") ]
            match search.text with
            | Some text -> "l.data ->> 'text' ILIKE @text", [ "@text", like text ]
            | None -> ()
        ]
        connection ()
        |> Sql.query $"
            {viewSql}
             WHERE l.data ->> 'isExpired' = 'false' AND l.data ->> 'isLegacy' = 'false'
               {searchSql searches}"
        |> Sql.parameters (searches |> List.collect snd)
        |> Sql.executeAsync toListingForView


/// Profile data access functions
[<RequireQualifiedAccess>]
module Profiles =
    
    /// Count the current profiles
    let count () =
        connection ()
        |> Sql.query $"SELECT COUNT(id) AS the_count FROM {Table.Profile} WHERE data ->> 'isLegacy' = 'false'"
        |> Sql.executeRowAsync (fun row -> row.int64 "the_count")
    
    /// Delete a profile by its ID
    let deleteById citizenId = backgroundTask {
        let! _ =
            connection ()
            |> Sql.query $"DELETE FROM {Table.Profile} WHERE id = @id"
            |> Sql.parameters [ "@id", Sql.string (CitizenId.toString citizenId) ]
            |> Sql.executeNonQueryAsync
        ()
    }
    
    /// Find a profile by citizen ID
    let findById citizenId = backgroundTask {
        match! connection () |> getDocument<Profile> Table.Profile (CitizenId.toString citizenId) with
        | Some profile when not profile.IsLegacy -> return Some profile
        | Some _
        | None -> return None
    }
    
    /// Find a profile by citizen ID for viewing (includes citizen and continent information)
    let findByIdForView citizenId = backgroundTask {
        let! tryCitizen =
            connection ()
            |> Sql.query $"
                SELECT p.*, c.data AS cit_data, o.data AS cont_data
                  FROM {Table.Profile} p
                       INNER JOIN {Table.Citizen}   c ON c.id = p.id
                       INNER JOIN {Table.Continent} o ON o.id = p.data ->> 'continentId'
                 WHERE p.id                  = @id
                   AND p.data ->> 'isLegacy' = 'false'"
            |> Sql.parameters [ "@id", Sql.string (CitizenId.toString citizenId) ]
            |> Sql.executeAsync (fun row ->
                {   profile   = toDocument<Profile> row
                    citizen   = toDocumentFrom<Citizen>   "cit_data"  row
                    continent = toDocumentFrom<Continent> "cont_data" row
                })
        return List.tryHead tryCitizen
    }
    
    /// Save a profile
    let save (profile : Profile) =
        connection () |> saveDocument Table.Profile (CitizenId.toString profile.Id) <| mkDoc profile
    
    /// Search profiles (logged-on users)
    let search (search : ProfileSearch) = backgroundTask {
        let searches = [
            match search.continentId with
            | Some contId -> "p.data ->> 'continentId' = @continentId", [ "@continentId", Sql.string contId ]
            | None -> ()
            if search.remoteWork <> "" then
                "p.data ->> 'remoteWork' = @remote", [ "@remote", jsonBool (search.remoteWork = "yes") ]
            match search.skill with
            | Some skl -> "p.data -> 'skills' ->> 'description' ILIKE @description", [ "@description", like skl ]
            | None -> ()
            match search.bioExperience with
            | Some text ->
                "(p.data ->> 'biography' ILIKE @text OR p.data ->> 'experience' ILIKE @text)",
                [ "@text", Sql.string text ]
            | None -> ()
        ]
        let! results =
            connection ()
            |> Sql.query $"
                SELECT p.*, c.data AS cit_data
                  FROM {Table.Profile} p
                       INNER JOIN {Table.Citizen} c ON c.id = p.id
                 WHERE p.data ->> 'isLegacy' = 'false'
                   {searchSql searches}"
            |> Sql.parameters (searches |> List.collect snd)
            |> Sql.executeAsync (fun row ->
                let profile = toDocument<Profile> row
                let citizen = toDocumentFrom<Citizen> "cit_data" row
                {   citizenId         = profile.Id
                    displayName       = Citizen.name citizen
                    seekingEmployment = profile.IsSeekingEmployment
                    remoteWork        = profile.IsRemote
                    fullTime          = profile.IsFullTime
                    lastUpdatedOn     = profile.LastUpdatedOn
                })
        return results |> List.sortBy (fun psr -> psr.displayName.ToLowerInvariant ())
    }

    // Search profiles (public)
    let publicSearch (search : PublicSearch) =
        let searches = [
            match search.continentId with
            | Some contId -> "p.data ->> 'continentId' = @continentId", [ "@continentId", Sql.string contId ]
            | None -> ()
            match search.region with
            | Some region -> "p.data ->> 'region' ILIKE @region", [ "@region", like region ]
            | None -> ()
            if search.remoteWork <> "" then
                "p.data ->> 'remoteWork' = @remote", [ "@remote", jsonBool (search.remoteWork = "yes") ]
            match search.skill with
            | Some skl ->
                "p.data -> 'skills' ->> 'description' ILIKE @description", [ "@description", like skl ]
            | None -> ()
        ]
        connection ()
        |> Sql.query $"
            SELECT p.*, c.data AS cont_data
              FROM {Table.Profile} p
                   INNER JOIN {Table.Continent} c ON c.id = p.data ->> 'continentId'
             WHERE p.data ->> 'isPublic' = 'true'
               AND p.data ->> 'isLegacy' = 'false'
               {searchSql searches}"
        |> Sql.executeAsync (fun row ->
            let profile = toDocument<Profile> row
            let continent = toDocumentFrom<Continent> "cont_data" row
            {   continent  = continent.Name
                region     = profile.Region
                remoteWork = profile.IsRemote
                skills     = profile.Skills
                             |> List.map (fun s ->
                                 let notes = match s.Notes with Some n -> $" ({n})" | None -> ""
                                 $"{s.Description}{notes}")
            })

/// Success story data access functions
[<RequireQualifiedAccess>]
module Successes =
    
    // Retrieve all success stories  
    let all () =
        connection ()
        |> Sql.query $"
            SELECT s.*, c.data AS cit_data
              FROM {Table.Success} s
                   INNER JOIN {Table.Citizen} c ON c.id = s.data ->> 'citizenId'
             ORDER BY s.data ->> 'recordedOn' DESC"
        |> Sql.executeAsync (fun row ->
            let success = toDocument<Success> row
            let citizen = toDocumentFrom<Citizen> "cit_data" row
            {   id          = success.Id
                citizenId   = success.CitizenId
                citizenName = Citizen.name citizen
                recordedOn  = success.RecordedOn
                fromHere    = success.IsFromHere
                hasStory    = Option.isSome success.Story
            })
    
    /// Find a success story by its ID
    let findById successId =
        connection () |> getDocument<Success> Table.Success (SuccessId.toString successId)
    
    /// Save a success story
    let save (success : Success) =
        connection () |> saveDocument Table.Success (SuccessId.toString success.Id) <| mkDoc success
    