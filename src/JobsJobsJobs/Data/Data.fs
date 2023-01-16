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

/// Connection management for the document store
module DataConnection =
    
    open Microsoft.Extensions.Configuration
    open Npgsql
    
    /// The data source for the document store
    let mutable private dataSource : NpgsqlDataSource option = None
    
    /// Get a connection
    let connection () =
        match dataSource with
        | Some ds -> ds.OpenConnection () |> Sql.existingConnection
        | None -> invalidOp "Connection.setUp() must be called before accessing the database"
    
    /// Create tables
    let private createTables () = backgroundTask {
        let sql = [
            "CREATE SCHEMA IF NOT EXISTS jjj"
            // Tables
            $"CREATE TABLE IF NOT EXISTS {Table.Citizen}      (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL)"
            $"CREATE TABLE IF NOT EXISTS {Table.Continent}    (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL)"
            $"CREATE TABLE IF NOT EXISTS {Table.Listing}      (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL)"
            $"CREATE TABLE IF NOT EXISTS {Table.Profile}      (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL,
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
        ]
        let! _ =
            connection ()
            |> Sql.executeTransactionAsync (sql |> List.map (fun sql -> sql, [ [] ]))
        ()
    }
    
    /// Set up the data connection from the given configuration
    let setUp (cfg : IConfiguration) = backgroundTask {
        let builder = NpgsqlDataSourceBuilder (cfg.GetConnectionString "PostgreSQL")
        let _ = builder.UseNodaTime ()
        dataSource <- Some (builder.Build ())
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
    
    open NodaTime

    /// The last time a token purge check was run
    let mutable private lastPurge = Instant.MinValue
    
    /// Lock access to the above
    let private locker = obj ()
    
    /// Delete a citizen by their ID using the given connection properties
    let private doDeleteById citizenId connProps = backgroundTask {
        let! _ =
            connProps
            |> Sql.query $"
                DELETE FROM {Table.Success} WHERE data ->> 'citizenId' = @id;
                DELETE FROM {Table.Listing} WHERE data ->> 'citizenId' = @id;
                DELETE FROM {Table.Citizen} WHERE id                   = @id"
            |> Sql.parameters [ "@id", Sql.string (CitizenId.toString citizenId) ]
            |> Sql.executeNonQueryAsync
        ()
    }
    
    /// Delete a citizen by their ID
    let deleteById citizenId =
        doDeleteById citizenId (connection ())
    
    /// Save a citizen
    let private saveCitizen (citizen : Citizen) connProps =
        saveDocument Table.Citizen (CitizenId.toString citizen.Id) connProps (mkDoc citizen) 
    
    /// Save security information for a citizen
    let private saveSecurity (security : SecurityInfo) connProps =
        saveDocument Table.SecurityInfo (CitizenId.toString security.Id) connProps (mkDoc security)
    
    /// Purge expired tokens
    let private purgeExpiredTokens now = backgroundTask {
        let connProps = connection ()
        let! info =
            Sql.query $"SELECT * FROM {Table.SecurityInfo} WHERE data ->> 'tokenExpires' IS NOT NULL" connProps
            |> Sql.executeAsync toDocument<SecurityInfo>
        for expired in info |> List.filter (fun it -> it.TokenExpires.Value < now) do
            if expired.TokenUsage.Value = "confirm" then
                // Unconfirmed account; delete the entire thing
                do! doDeleteById expired.Id connProps
            else
                // Some other use; just clear the token
                do! saveSecurity { expired with Token = None; TokenUsage = None; TokenExpires = None } connProps
    }
    
    /// Check for tokens to purge if it's been more than 10 minutes since we last checked
    let private checkForPurge skipCheck =
        lock locker (fun () -> backgroundTask {
            let now  = SystemClock.Instance.GetCurrentInstant ()
            if skipCheck || (now - lastPurge).TotalMinutes >= 10 then
                do! purgeExpiredTokens now
                lastPurge <- now
        })
    
    /// Find a citizen by their ID
    let findById citizenId = backgroundTask {
        match! connection () |> getDocument<Citizen> Table.Citizen (CitizenId.toString citizenId) with
        | Some c when not c.IsLegacy -> return Some c
        | Some _
        | None -> return None
    }
    
    /// Save a citizen
    let save citizen =
        saveCitizen citizen (connection ())
        
    /// Register a citizen (saves citizen and security settings); returns false if the e-mail is already taken
    let register citizen (security : SecurityInfo) = backgroundTask {
        let  connProps = connection ()
        use  conn      = Sql.createConnection connProps
        use! txn       = conn.BeginTransactionAsync ()
        try
            do! saveCitizen  citizen  connProps
            do! saveSecurity security connProps
            do! txn.CommitAsync ()
            return true
        with
        | :? Npgsql.PostgresException as ex when ex.SqlState = "23505" && ex.ConstraintName = "uk_citizen_email" ->
            do! txn.RollbackAsync ()
            return false
    }
    
    /// Try to find the security information matching a confirmation token
    let private tryConfirmToken token connProps = backgroundTask {
        let! tryInfo =
            connProps
            |> Sql.query $"
                SELECT *
                  FROM {Table.SecurityInfo}
                 WHERE data ->> 'token'      = @token
                   AND data ->> 'tokenUsage' = 'confirm'"
            |> Sql.parameters [ "@token", Sql.string token ]
            |> Sql.executeAsync toDocument<SecurityInfo>
        return List.tryHead tryInfo
    }
    
    /// Confirm a citizen's account
    let confirmAccount token = backgroundTask {
        do! checkForPurge true
        let connProps = connection ()
        match! tryConfirmToken token connProps with
        | Some info ->
            do! saveSecurity { info with AccountLocked = false; Token = None; TokenUsage = None; TokenExpires = None }
                    connProps
            return true
        | None -> return false
    }
        
    /// Deny a citizen's account (user-initiated; used if someone used their e-mail address without their consent)
    let denyAccount token = backgroundTask {
        do! checkForPurge true
        let connProps = connection ()
        match! tryConfirmToken token connProps with
        | Some info ->
            do! doDeleteById info.Id connProps
            return true
        | None -> return false
    }
        
    /// Attempt a user log on
    let tryLogOn email password (pwVerify : Citizen -> string -> bool option) (pwHash : Citizen -> string -> string)
            now = backgroundTask {
        do! checkForPurge false
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
            else
                match pwVerify citizen password with
                | Some rehash ->
                    let hash = if rehash then pwHash citizen password else citizen.PasswordHash
                    do! saveSecurity { info with FailedLogOnAttempts = 0 } connProps
                    do! saveCitizen { citizen with LastSeenOn = now; PasswordHash = hash } connProps
                    return Ok { citizen with LastSeenOn = now }
                | None ->
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
        |> Sql.query $"SELECT * FROM {Table.Continent} ORDER BY data ->> 'name'"
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
        $"SELECT l.*, c.data ->> 'name' AS continent_name, u.data AS cit_data
            FROM {Table.Listing} l
                 INNER JOIN {Table.Continent} c ON c.id = l.data ->> 'continentId'
                 INNER JOIN {Table.Citizen}   u ON u.id = l.data ->> 'citizenId'"
    
    /// Map a result for a listing view
    let private toListingForView row =
        {   Listing       = toDocument<Listing> row
            ContinentName = row.string "continent_name"
            ListedBy      = Citizen.name (toDocumentFrom<Citizen> "cit_data" row)
        }
    
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
            |> Sql.query $"{viewSql} WHERE l.id = @id AND l.data ->> 'isLegacy' = 'false'"
            |> Sql.parameters [ "@id", Sql.string (ListingId.toString listingId) ]
            |> Sql.executeAsync toListingForView
        return List.tryHead tryListing
    }
    
    /// Save a listing
    let save (listing : Listing) =
        connection () |> saveDocument Table.Listing (ListingId.toString listing.Id) <| mkDoc listing
    
    /// Search job listings
    let search (search : ListingSearchForm) =
        let searches = [
            if search.ContinentId <> "" then
                "l.data ->> 'continentId' = @continentId", [ "@continentId", Sql.string search.ContinentId ]
            if search.Region <> "" then
                "l.data ->> 'region' ILIKE @region", [ "@region", like search.Region ]
            if search.RemoteWork <> "" then
                "l.data ->> 'isRemote' = @remote", [ "@remote", jsonBool (search.RemoteWork = "yes") ]
            if search.Text <> "" then
                "l.data ->> 'text' ILIKE @text", [ "@text", like search.Text ]
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
                {   Profile   = toDocument<Profile> row
                    Citizen   = toDocumentFrom<Citizen>   "cit_data"  row
                    Continent = toDocumentFrom<Continent> "cont_data" row
                })
        return List.tryHead tryCitizen
    }
    
    /// Save a profile
    let save (profile : Profile) =
        connection () |> saveDocument Table.Profile (CitizenId.toString profile.Id) <| mkDoc profile
    
    /// Search profiles (logged-on users)
    let search (search : ProfileSearchForm) = backgroundTask {
        let searches = [
            if search.ContinentId <> "" then
                "p.data ->> 'continentId' = @continentId", [ "@continentId", Sql.string search.ContinentId ]
            if search.RemoteWork <> "" then
                "p.data ->> 'isRemote' = @remote", [ "@remote", jsonBool (search.RemoteWork = "yes") ]
            if search.Skill <> "" then
                "EXISTS (
                    SELECT 1 FROM jsonb_array_elements(p.data['skills']) x(elt)
                     WHERE x ->> 'description' ILIKE @description)",
                [ "@description", like search.Skill ]
            if search.BioExperience <> "" then
                "(p.data ->> 'biography' ILIKE @text OR p.data ->> 'experience' ILIKE @text)",
                [ "@text", like search.BioExperience ]
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
                {   CitizenId         = profile.Id
                    DisplayName       = Citizen.name citizen
                    SeekingEmployment = profile.IsSeekingEmployment
                    RemoteWork        = profile.IsRemote
                    FullTime          = profile.IsFullTime
                    LastUpdatedOn     = profile.LastUpdatedOn
                })
        return results |> List.sortBy (fun psr -> psr.DisplayName.ToLowerInvariant ())
    }

    // Search profiles (public)
    let publicSearch (search : PublicSearchForm) =
        let searches = [
            if search.ContinentId <> "" then
                "p.data ->> 'continentId' = @continentId", [ "@continentId", Sql.string search.ContinentId ]
            if search.Region <> "" then
                "p.data ->> 'region' ILIKE @region", [ "@region", like search.Region ]
            if search.RemoteWork <> "" then
                "p.data ->> 'isRemote' = @remote", [ "@remote", jsonBool (search.RemoteWork = "yes") ]
            if search.Skill <> "" then
                "EXISTS (
                    SELECT 1 FROM jsonb_array_elements(p.data['skills']) x(elt)
                     WHERE x ->> 'description' ILIKE @description)",
                [ "@description", like search.Skill ]
        ]
        connection ()
        |> Sql.query $"
            SELECT p.*, c.data AS cont_data
              FROM {Table.Profile} p
                   INNER JOIN {Table.Continent} c ON c.id = p.data ->> 'continentId'
             WHERE p.data ->> 'isPubliclySearchable' = 'true'
               AND p.data ->> 'isLegacy'             = 'false'
               {searchSql searches}"
        |> Sql.parameters (searches |> List.collect snd)
        |> Sql.executeAsync (fun row ->
            let profile = toDocument<Profile> row
            let continent = toDocumentFrom<Continent> "cont_data" row
            {   Continent  = continent.Name
                Region     = profile.Region
                RemoteWork = profile.IsRemote
                Skills     = profile.Skills
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
            {   Id          = success.Id
                CitizenId   = success.CitizenId
                CitizenName = Citizen.name citizen
                RecordedOn  = success.RecordedOn
                FromHere    = success.IsFromHere
                HasStory    = Option.isSome success.Story
            })
    
    /// Find a success story by its ID
    let findById successId =
        connection () |> getDocument<Success> Table.Success (SuccessId.toString successId)
    
    /// Save a success story
    let save (success : Success) =
        connection () |> saveDocument Table.Success (SuccessId.toString success.Id) <| mkDoc success
    