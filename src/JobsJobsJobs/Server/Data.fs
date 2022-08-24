/// Data access functions for Jobs, Jobs, Jobs
module JobsJobsJobs.Api.Data

open JobsJobsJobs.Domain

/// JSON converters used with RethinkDB persistence
module Converters =
  
    open Microsoft.FSharpLu.Json
    open Newtonsoft.Json
    open System

    /// JSON converter for citizen IDs
    type CitizenIdJsonConverter() =
        inherit JsonConverter<CitizenId>()
        override _.WriteJson(writer : JsonWriter, value : CitizenId, _ : JsonSerializer) =
            writer.WriteValue (CitizenId.toString value)
        override _.ReadJson(reader: JsonReader, _ : Type, _ : CitizenId, _ : bool, _ : JsonSerializer) =
            (string >> CitizenId.ofString) reader.Value
    
    /// JSON converter for continent IDs
    type ContinentIdJsonConverter() =
        inherit JsonConverter<ContinentId>()
        override _.WriteJson(writer : JsonWriter, value : ContinentId, _ : JsonSerializer) =
            writer.WriteValue (ContinentId.toString value)
        override _.ReadJson(reader: JsonReader, _ : Type, _ : ContinentId, _ : bool, _ : JsonSerializer) =
            (string >> ContinentId.ofString) reader.Value

    /// JSON converter for Markdown strings
    type MarkdownStringJsonConverter() =
        inherit JsonConverter<MarkdownString>()
        override _.WriteJson(writer : JsonWriter, value : MarkdownString, _ : JsonSerializer) =
            writer.WriteValue (MarkdownString.toString value)
        override _.ReadJson(reader: JsonReader, _ : Type, _ : MarkdownString, _ : bool, _ : JsonSerializer) =
            (string >> Text) reader.Value

    /// JSON converter for listing IDs
    type ListingIdJsonConverter() =
        inherit JsonConverter<ListingId>()
        override _.WriteJson(writer : JsonWriter, value : ListingId, _ : JsonSerializer) =
            writer.WriteValue (ListingId.toString value)
        override _.ReadJson(reader: JsonReader, _ : Type, _ : ListingId, _ : bool, _ : JsonSerializer) =
            (string >> ListingId.ofString) reader.Value

    /// JSON converter for skill IDs
    type SkillIdJsonConverter() =
        inherit JsonConverter<SkillId>()
        override _.WriteJson(writer : JsonWriter, value : SkillId, _ : JsonSerializer) =
            writer.WriteValue (SkillId.toString value)
        override _.ReadJson(reader: JsonReader, _ : Type, _ : SkillId, _ : bool, _ : JsonSerializer) =
            (string >> SkillId.ofString) reader.Value
    
    /// JSON converter for success report IDs
    type SuccessIdJsonConverter() =
        inherit JsonConverter<SuccessId>()
        override _.WriteJson(writer : JsonWriter, value : SuccessId, _ : JsonSerializer) =
            writer.WriteValue (SuccessId.toString value)
        override _.ReadJson(reader: JsonReader, _ : Type, _ : SuccessId, _ : bool, _ : JsonSerializer) =
            (string >> SuccessId.ofString) reader.Value
    
    /// All JSON converters needed for the application
    let all () : JsonConverter list =
        [ CitizenIdJsonConverter      ()
          ContinentIdJsonConverter    ()
          MarkdownStringJsonConverter ()
          ListingIdJsonConverter      ()
          SkillIdJsonConverter        ()
          SuccessIdJsonConverter      ()
          CompactUnionJsonConverter   ()
        ]


/// Table names
[<RequireQualifiedAccess>]
module Table =
    
    /// The user (citizen of Gitmo Nation) table
    let Citizen = "citizen"
    
    /// The continent table
    let Continent = "continent"
    
    /// The citizen employment profile table
    let Profile = "profile"
    
    /// The success story table
    let Success = "success"
    
    /// All tables
    let all () = [ Citizen; Continent; Profile; Success ]

open NodaTime
open Npgsql
open Npgsql.FSharp



open RethinkDb.Driver.FSharp.Functions
open RethinkDb.Driver.Net

/// Reconnection functions (if the RethinkDB driver has a network error, it will not reconnect on its own)
[<AutoOpen>]
module private Reconnect =
  
    /// Retrieve a result using the F# driver's default retry policy
    let result<'T> conn expr = runResult<'T> expr |> withRetryDefault |> withConn conn
    
    /// Retrieve an optional result using the F# driver's default retry policy
    let resultOption<'T> conn expr = runResult<'T> expr |> withRetryDefault |> asOption |> withConn conn
    
    /// Write a query using the F# driver's default retry policy, ignoring the result
    let write conn expr = runWrite expr |> withRetryDefault |> ignoreResult |> withConn conn
    

open RethinkDb.Driver.Ast
open Marten

/// Shorthand for the RethinkDB R variable (how every command starts)
let private r = RethinkDb.Driver.RethinkDB.R

/// Functions run at startup
[<RequireQualifiedAccess>]
module Startup =
  
    open Microsoft.Extensions.Configuration
    open Microsoft.Extensions.Logging
    open NodaTime.Serialization.JsonNet
    open RethinkDb.Driver.FSharp
    
    /// Create a RethinkDB connection
    let createConnection (cfg : IConfigurationSection) (log : ILogger) =
        // Add all required JSON converters
        Converter.Serializer.ConfigureForNodaTime DateTimeZoneProviders.Tzdb |> ignore
        Converters.all ()
        |> List.iter Converter.Serializer.Converters.Add
        // Connect to the database
        let config = DataConfig.FromConfiguration cfg
        log.LogInformation $"Connecting to rethinkdb://{config.Hostname}:{config.Port}/{config.Database}"
        config.CreateConnection ()

    /// Ensure the tables and indexes that are required exist
    let establishEnvironment (log : ILogger) conn = task {
        
        let! tables =
            Sql.existingConnection conn
            |> Sql.query "SELECT tablename FROM pg_tables WHERE schemaname = 'jjj'"
            |> Sql.executeAsync (fun row -> row.string "tablename")
        let needsTable table = not (List.contains table tables)
        
        let sql = seq {
            if needsTable "continent" then
                "CREATE TABLE jjj.continent (
                    id   UUID NOT NULL PRIMARY KEY,
                    name TEXT NOT NULL)"
            if needsTable "citizen" then
                "CREATE TABLE jjj.citizen (
                    id             UUID        NOT NULL PRIMARY KEY,
                    joined_on      TIMESTAMPTZ NOT NULL,
                    last_seen_on   TIMESTAMPTZ NOT NULL,
                    email          TEXT        NOT NULL UNIQUE,
                    first_name     TEXT        NOT NULL,
                    last_name      TEXT        NOT NULL,
                    password_hash  TEXT        NOT NULL,
                    is_legacy      BOOLEAN     NOT NULL,
                    display_name   TEXT,
                    other_contacts TEXT)"
            if needsTable "listing" then
                "CREATE TABLE jjj.listing (
                    id              UUID        NOT NULL PRIMARY KEY,
                    citizen_id      UUID        NOT NULL,
                    created_on      TIMESTAMPTZ NOT NULL,
                    title           TEXT        NOT NULL,
                    continent_id    UUID        NOT NULL,
                    region          TEXT        NOT NULL,
                    is_remote       BOOLEAN     NOT NULL,
                    is_expired      BOOLEAN     NOT NULL,
                    updated_on      TIMESTAMPTZ NOT NULL,
                    listing_text    TEXT        NOT NULL,
                    needed_by       DATE,
                    was_filled_here BOOLEAN,
                    FOREIGN KEY fk_listing_citizen   (citizen_id)   REFERENCES jjj.citizen   (id) ON DELETE CASCADE,
                    FOREIGN KEY fk_listing_continent (continent_id) REFERENCES jjj.continent (id))"
                "CREATE INDEX idx_listing_citizen   ON jjj.listing (citizen_id)"
                "CREATE INDEX idx_listing_continent ON jjj.listing (continent_id)"
            if needsTable "profile" then
                "CREATE TABLE jjj.profile (
                    citizen_id             UUID        NOT NULL PRIMARY KEY,
                    is_seeking             BOOLEAN     NOT NULL,
                    is_public_searchable   BOOLEAN     NOT NULL,
                    is_public_linkable     BOOLEAN     NOT NULL,
                    continent_id           UUID        NOT NULL,
                    region                 TEXT        NOT NULL,
                    is_available_remotely  BOOLEAN     NOT NULL,
                    is_available_full_time BOOLEAN     NOT NULL,
                    biography              TEXT        NOT NULL,
                    last_updated_on        TIMESTAMPTZ NOT NULL,
                    experience             TEXT,
                    FOREIGN KEY fk_profile_citizen   (citizen_id)   REFERENCES jjj.citizen   (id) ON DELETE CASCADE,
                    FOREIGN KEY fk_profile_continent (continent_id) REFERENCES jjj.continent (id))"
                "CREATE INDEX idx_profile_citizen   ON jjj.profile (citizen_id)"
                "CREATE INDEX idx_profile_continent ON jjj.profile (continent_id)"
                "CREATE TABLE jjj.profile_skill (
                    id          UUID NOT NULL PRIMARY KEY,
                    citizen_id  UUID NOT NULL,
                    description TEXT NOT NULL,
                    notes       TEXT,
                    FOREIGN KEY fk_profile_skill_profile (citizen_id) REFERENCES jjj.profile (citizen_id)
                        ON DELETE CASCADE)"
                "CREATE INDEX idx_profile_skill_profile ON jjj.profile_skill (citizen_id)"
            if needsTable "security_info" then
                "CREATE TABLE jjj.security_info (
                    id              UUID     NOT NULL PRIMARY KEY,
                    failed_attempts SMALLINT NOT NULL,
                    is_locked       BOOLEAN  NOT NULL,
                    token           TEXT,
                    token_usage     TEXT,
                    token_expires   TIMESTAMPTZ,
                    FOREIGN KEY fk_security_info_citizen (id) REFERENCES jjj.citizen (id) ON DELETE CASCADE)"
                "CREATE INDEX idx_security_info_expires ON jjj.security_info (token_expires)"
            if needsTable "success" then
                "CREATE TABLE jjj.success (
                    id            UUID        NOT NULL PRIMARY KEY,
                    citizen_id    UUID        NOT NULL,
                    recorded_on   TIMESTAMPTZ NOT NULL,
                    was_from_here BOOLEAN     NOT NULL,
                    source        TEXT        NOT NULL,
                    story         TEXT,
                    FOREIGN KEY fk_success_citizen (citizen_id) REFERENCES jjj.citizen (id) ON DELETE CASCADE)"
                "CREATE INDEX idx_success_citizen ON jjj.success (citizen_id)"
        }
        if not (Seq.isEmpty sql) then
            let! _ =
                Sql.existingConnection conn
                |> Sql.executeTransactionAsync
                    (sql
                     |> Seq.map (fun it ->
                         let parts = it.Split ' '
                         log.LogInformation $"Creating {parts[2]} {parts[1].ToLowerInvariant ()}..."
                         it, [ [] ])
                     |> List.ofSeq)
            ()
    }


open JobsJobsJobs.Domain.SharedTypes

/// Sanitize user input, and create a "contains" pattern for use with RethinkDB queries
let private regexContains = System.Text.RegularExpressions.Regex.Escape >> sprintf "(?i)%s"

/// Apply filters to a query, ensuring that types all match up
let private applyFilters (filters : (ReqlExpr -> Filter) list) query : ReqlExpr =
    if List.isEmpty filters then
        query
    else
        let first = List.head filters query
        List.fold (fun q (f : ReqlExpr -> Filter) -> f q) first (List.tail filters)

/// Derive a user's display name from real name, display name, or handle (in that order)
let private deriveDisplayName (it : ReqlExpr) =
    r.Branch (it.G("realName"   ).Default_("").Ne "", it.G "realName",
              it.G("displayName").Default_("").Ne "", it.G "displayName",
                                                      it.G "mastodonUser")

/// Custom SQL parameter functions
module Sql =
    
    /// Create a citizen ID parameter
    let citizenId = CitizenId.value >> Sql.uuid
    
    /// Create a continent ID parameter
    let continentId = ContinentId.value >> Sql.uuid
    
    /// Create a listing ID parameter
    let listingId = ListingId.value >> Sql.uuid
    
    /// Create a Markdown string parameter
    let markdown = MarkdownString.toString >> Sql.string
    
    /// Create a parameter for the given value
    let param<'T> name (value : 'T) =
        name, Sql.parameter (NpgsqlParameter (name, value))

    /// Create a parameter for a possibly-missing value
    let paramOrNone<'T> name (value : 'T option) =
        name, Sql.parameter (NpgsqlParameter (name, if Option.isSome value then box value else System.DBNull.Value))
    
    /// Create a skill ID parameter
    let skillId = SkillId.value >> Sql.uuid
    
    /// Create a success ID parameter
    let successId = SuccessId.value >> Sql.uuid

    
/// Map data results to domain types
module Map =
    
    /// Create a citizen from a data row
    let toCitizen (row : RowReader) : Citizen =
        {   id            = (row.uuid >> CitizenId) "id"
            joinedOn      = row.fieldValue<Instant> "joined_on"
            lastSeenOn    = row.fieldValue<Instant> "last_seen_on"
            email         = row.string              "email"
            firstName     = row.string              "first_name"
            lastName      = row.string              "last_name"
            passwordHash  = row.string              "password_hash"
            displayName   = row.stringOrNone        "display_name"
            // TODO: deserialize from JSON
            otherContacts = [] // row.stringOrNone        "other_contacts"
            isLegacy      = false
        }
        
    /// Create a continent from a data row
    let toContinent (row : RowReader) : Continent =
        {   id   = (row.uuid >> ContinentId) "continent_id"
            name = row.string                "continent_name"
        }
    
    /// Extract a count from a row
    let toCount (row : RowReader) =
        row.int64 "the_count"
    
    /// Create a job listing from a data row
    let toListing (row : RowReader) : Listing =
        {   id            = (row.uuid >> ListingId)         "id"
            citizenId     = (row.uuid >> CitizenId)         "citizen_id"
            createdOn     = row.fieldValue<Instant>         "created_on"
            title         = row.string                      "title"
            continentId   = (row.uuid >> ContinentId)       "continent_id"
            region        = row.string                      "region"
            remoteWork    = row.bool                        "is_remote"
            isExpired     = row.bool                        "is_expired"
            updatedOn     = row.fieldValue<Instant>         "updated_on"
            text          = (row.string >> Text)            "listing_text"
            neededBy      = row.fieldValueOrNone<LocalDate> "needed_by"
            wasFilledHere = row.boolOrNone                  "was_filled_here"
            isLegacy      = false
        }
    
    /// Create a job listing for viewing from a data row
    let toListingForView (row : RowReader) : ListingForView =
        {   listing   = toListing   row
            continent = toContinent row
        }
    
    /// Create a profile from a data row
    let toProfile (row : RowReader) : Profile =
        {   id                = (row.uuid >> CitizenId)   "citizen_id"
            seekingEmployment = row.bool                  "is_seeking"
            isPublic          = row.bool                  "is_public_searchable"
            isPublicLinkable  = row.bool                  "is_public_linkable"
            continentId       = (row.uuid >> ContinentId) "continent_id"
            region            = row.string                "region"
            remoteWork        = row.bool                  "is_available_remotely"
            fullTime          = row.bool                  "is_available_full_time"
            biography         = (row.string >> Text)      "biography"
            lastUpdatedOn     = row.fieldValue<Instant>   "last_updated_on"
            experience        = row.stringOrNone          "experience" |> Option.map Text
            skills            = []
            isLegacy          = false
        }
    
    /// Create a skill from a data row
    let toSkill (row : RowReader) : Skill =
        {   id          = (row.uuid >> SkillId) "id"
            description = row.string            "description"
            notes       = row.stringOrNone      "notes"
        }
    
    /// Create a success story from a data row
    let toSuccess (row : RowReader) : Success =
        {   id         = (row.uuid >> SuccessId) "id"
            citizenId  = (row.uuid >> CitizenId) "citizen_id"
            recordedOn = row.fieldValue<Instant> "recorded_on"
            fromHere   = row.bool                "was_from_here"
            source     = row.string              "source"
            story      = row.stringOrNone        "story" |> Option.map Text
        }


/// Convert a possibly-null record type to an option
let optional<'T> (value : 'T) = if isNull (box value) then None else Some value

open System
open System.Linq

/// Profile data access functions
[<RequireQualifiedAccess>]
module Profile =

    /// Count the current profiles
    let count (session : IQuerySession) =
        session.Query<Profile>().Where(fun p -> not p.isLegacy).LongCountAsync ()

    /// Find a profile by citizen ID
    let findById citizenId (session : IQuerySession) = backgroundTask {
        let! profile = session.LoadAsync<Profile> (CitizenId.value citizenId)
        return
            match optional profile with
            | Some p when not p.isLegacy -> Some p
            | Some _
            | None -> None
    }
    
    /// Insert or update a profile
    [<Obsolete "Inline this">]
    let save (profile : Profile) (session : IDocumentSession) =
        session.Store profile
    
    /// Delete a citizen's profile
    let delete citizenId conn = backgroundTask {
        let! _ =
            Sql.existingConnection conn
            |> Sql.query "DELETE FROM jjj.profile WHERE citizen_id = @id"
            |> Sql.parameters [ "@id", Sql.citizenId citizenId ]
            |> Sql.executeNonQueryAsync
        ()
    }
  
    /// Search profiles (logged-on users)
    let search (search : ProfileSearch) conn =
        fromTable Table.Profile
        |> eqJoin "id" (fromTable Table.Citizen)
        |> without [ "right.id" ]
        |> zip
        |> applyFilters
            [ match search.continentId with
              | Some contId -> yield filter {| continentId = ContinentId.ofString contId |}
              | None -> ()
              match search.remoteWork with
              | "" -> ()
              | _ -> yield filter {| remoteWork = search.remoteWork = "yes" |}
              match search.skill with
              | Some skl ->
                  yield filterFunc (fun it ->
                      it.G("skills").Contains (ReqlFunction1 (fun s -> s.G("description").Match (regexContains skl))))
              | None -> ()
              match search.bioExperience with
              | Some text ->
                  let txt = regexContains text
                  yield filterFunc (fun it -> it.G("biography").Match(txt).Or (it.G("experience").Match txt))
              | None -> ()
            ]
        |> mergeFunc (fun it -> {| displayName = deriveDisplayName it; citizenId = it.G "id" |})
        |> pluck [ "citizenId"; "displayName"; "seekingEmployment"; "remoteWork"; "fullTime"; "lastUpdatedOn" ]
        |> orderByFunc (fun it -> it.G("displayName").Downcase ())
        |> result<ProfileSearchResult list> conn

    // Search profiles (public)
    let publicSearch (search : PublicSearch) conn =
        fromTable Table.Profile
        |> eqJoin "continentId" (fromTable Table.Continent)
        |> without [ "right.id" ]
        |> zip
        |> applyFilters
            [ yield filter {| isPublic = true |}
              match search.continentId with
              | Some contId -> yield filter {| continentId = ContinentId.ofString contId |}
              | None -> ()
              match search.region with
              | Some reg -> yield filterFunc (fun it -> it.G("region").Match (regexContains reg))
              | None -> ()
              match search.remoteWork with
              | "" -> ()
              | _ -> yield filter {| remoteWork = search.remoteWork = "yes" |}
              match search.skill with
              | Some skl ->
                  yield filterFunc (fun it ->
                      it.G("skills").Contains (ReqlFunction1 (fun s -> s.G("description").Match (regexContains skl))))
              | None -> ()
            ]
        |> mergeFunc (fun it ->
            {|  skills    = it.G("skills").Map (ReqlFunction1 (fun skill ->
                                r.Branch(skill.G("notes").Default_("").Eq "", skill.G "description",
                                         skill.G("description").Add(" (").Add(skill.G("notes")).Add ")")))
                continent = it.G "name"
            |})
        |> pluck [ "continent"; "region"; "skills"; "remoteWork" ]
        |> result<PublicSearchResult list> conn

/// Citizen data access functions
[<RequireQualifiedAccess>]
module Citizen =
  
    /// Find a citizen by their ID
    let findById citizenId (session : IQuerySession) = backgroundTask {
        let! citizen = session.LoadAsync<Citizen> (CitizenId.value citizenId)
        return
            match optional citizen with
            | Some c when not c.isLegacy -> Some c
            | Some _
            | None -> None
    }

    /// Find a citizen by their e-mail address
    let findByEmail email conn = backgroundTask {
        let! citizen =
            Sql.existingConnection conn
            |> Sql.query "SELECT * FROM jjj.citizen WHERE email = @email AND is_legacy = FALSE"
            |> Sql.parameters [ "@email", Sql.string email ]
            |> Sql.executeAsync Map.toCitizen
        return List.tryHead citizen
    }
  
    /// Add or update a citizen
    let save (citizen : Citizen) conn = backgroundTask {
        let! _ =
            Sql.existingConnection conn
            |> Sql.query
                "INSERT INTO jjj.citizen (
                    id, joined_on, last_seen_on, email, first_name, last_name, password_hash, display_name,
                    other_contacts, is_legacy
                ) VALUES (
                    @id, @joinedOn, @lastSeenOn, @email, @firstName, @lastName, @passwordHash, @displayName,
                    @otherContacts, FALSE
                ) ON CONFLICT (id) DO UPDATE
                SET email          = EXCLUDED.email,
                    first_name     = EXCLUDED.first_name,
                    last_name      = EXCLUDED.last_name,
                    password_hash  = EXCLUDED.password_hash,
                    display_name   = EXCLUDED.display_name,
                    other_contacts = EXCLUDED.other_contacts"
            |> Sql.parameters
                [   "@id",            Sql.citizenId    citizen.id
                    "@joinedOn"       |>Sql.param<|    citizen.joinedOn
                    "@lastSeenOn"     |>Sql.param<|    citizen.lastSeenOn
                    "@email",         Sql.string       citizen.email
                    "@firstName",     Sql.string       citizen.firstName
                    "@lastName",      Sql.string       citizen.lastName
                    "@passwordHash",  Sql.string       citizen.passwordHash
                    "@displayName",   Sql.stringOrNone citizen.displayName
                    "@otherContacts", Sql.stringOrNone (if List.isEmpty citizen.otherContacts then None else Some "")
                ]
            |> Sql.executeNonQueryAsync
        ()
    }

    /// Update the last seen on date for a citizen
    let logOnUpdate (citizen : Citizen) conn = backgroundTask {
        let! _ =
            Sql.existingConnection conn
            |> Sql.query "UPDATE jjj.citizen SET last_seen_on = @lastSeenOn WHERE id = @id"
            |> Sql.parameters [ "@id", Sql.citizenId citizen.id; "@lastSeenOn" |>Sql.param<| citizen.lastSeenOn ]
            |> Sql.executeNonQueryAsync
        ()
    }
  
    /// Delete a citizen
    let delete citizenId conn = backgroundTask {
        let! _ =
            Sql.existingConnection conn
            |> Sql.query "DELETE FROM citizen WHERE id = @id"
            |> Sql.parameters [ "@id", Sql.citizenId citizenId ]
            |> Sql.executeNonQueryAsync
        ()
    }
    

/// Continent data access functions
[<RequireQualifiedAccess>]
module Continent =

    /// Get all continents
    let all conn =
        Sql.existingConnection conn
        |> Sql.query "SELECT id AS continent_id, name AS continent_name FROM jjj.continent"
        |> Sql.executeAsync Map.toContinent
  
    /// Get a continent by its ID
    let findById contId conn = backgroundTask {
        let! continent =
            Sql.existingConnection conn
            |> Sql.query "SELECT id AS continent_id, name AS continent_name FROM jjj.continent WHERE id = @id"
            |> Sql.parameters [ "@id", Sql.continentId contId ]
            |> Sql.executeAsync Map.toContinent
        return List.tryHead continent
    }


/// Job listing data access functions
[<RequireQualifiedAccess>]
module Listing =  

    /// The SQL to select the listing and continent fields
    let private forViewSql =
        "SELECT l.*, c.name AS continent_name
           FROM jjj.listing l
                INNER JOIN jjj.continent c ON c.id = l.continent_id"
    
    /// Find all job listings posted by the given citizen
    let findByCitizen citizenId conn =
        Sql.existingConnection conn
        |> Sql.query $"{forViewSql} WHERE l.citizen_id = @citizenId"
        |> Sql.parameters [ "@citizenId", Sql.citizenId citizenId ]
        |> Sql.executeAsync Map.toListingForView
  
    /// Find a listing by its ID
    let findById listingId conn = backgroundTask {
        let! listing =
            Sql.existingConnection conn
            |> Sql.query "SELECT * FROM jjj.listing WHERE id = @id"
            |> Sql.parameters [ "@id", Sql.listingId listingId ]
            |> Sql.executeAsync Map.toListing
        return List.tryHead listing
    }
  
    /// Find a listing by its ID for viewing (includes continent information)
    let findByIdForView (listingId : ListingId) conn = backgroundTask {
        let! listing =
            Sql.existingConnection conn
            |> Sql.query $"{forViewSql} WHERE l.id = @id"
            |> Sql.parameters [ "@id", Sql.listingId listingId ]
            |> Sql.executeAsync Map.toListingForView
        return List.tryHead listing
    }
  
    /// Add or update a listing
    let save (listing : Listing) conn = backgroundTask {
        let! _ =
            Sql.existingConnection conn
            |> Sql.query
                "INSERT INTO jjj.listing (
                    id, citizen_id, created_on, title, continent_id, region, is_remote, is_expired, updated_on,
                    listing_text, needed_by, was_filled_here
                ) VALUES (
                    @id, @citizenId, @createdOn, @title, @continentId, @region, @isRemote, @isExpired, @updatedOn,
                    @text, @neededBy, @wasFilledHere
                ) ON CONFLICT (id) DO UPDATE
                SET title           = EXCLUDED.title,
                    continent_id    = EXCLUDED.continent_id,
                    region          = EXCLUDED.region,
                    is_remote       = EXCLUDED.is_remote,
                    is_expired      = EXCLUDED.is_expired,
                    updated_on      = EXCLUDED.updated_on,
                    listing_text    = EXCLUDED.listing_text,
                    needed_by       = EXCLUDED.needed_by,
                    was_filled_here = EXCLUDED.was_filled_here"
            |> Sql.parameters
                [   "@id",            Sql.listingId       listing.id
                    "@citizenId",     Sql.citizenId       listing.citizenId
                    "@createdOn"      |>Sql.param<|       listing.createdOn
                    "@title",         Sql.string          listing.title
                    "@continentId",   Sql.continentId     listing.continentId
                    "@region",        Sql.string          listing.region
                    "@isRemote",      Sql.bool            listing.remoteWork
                    "@isExpired",     Sql.bool            listing.isExpired
                    "@updatedOn"      |>Sql.param<|       listing.updatedOn
                    "@text",          Sql.markdown        listing.text
                    "@neededBy"       |>Sql.paramOrNone<| listing.neededBy
                    "@wasFilledHere", Sql.boolOrNone      listing.wasFilledHere
                    
                ]
            |> Sql.executeNonQueryAsync
        ()
    }
  
    /// Expire a listing
    let expire listingId fromHere (now : Instant) conn = backgroundTask {
        let! _ =
            Sql.existingConnection conn
            |> Sql.query
                "UPDATE jjj.listing
                    SET is_expired      = TRUE,
                        was_filled_here = @wasFilledHere,
                        updated_on      = @updatedOn
                  WHERE id = @id"
            |> Sql.parameters
                [   "@wasFilledHere", Sql.bool      fromHere
                    "@updatedOn"      |>Sql.param<| now
                    "@id",            Sql.listingId listingId
                ]
            |> Sql.executeNonQueryAsync
        ()
    }
    
    /// Search job listings
    let search (search : ListingSearch) conn =
        let filters = seq {
            match search.continentId with
            | Some contId ->
                "l.continent = @continentId", [ "@continentId", Sql.continentId (ContinentId.ofString contId) ]
            | None -> ()
            match search.region with
            | Some region -> "l.region ILIKE '%@region%'", [ "@region", Sql.string region ]
            | None -> ()
            if search.remoteWork <> "" then
                "l.is_remote = @isRemote", ["@isRemote", Sql.bool (search.remoteWork = "yes") ]
            match search.text with
            | Some text -> "l.listing_text ILIKE '%@text%'", [ "@text", Sql.string text ]
            | None -> ()
        }
        let filterSql = filters |> Seq.map fst |> String.concat " AND "
        Sql.existingConnection conn
        |> Sql.query $"{forViewSql} WHERE l.is_expired = FALSE{filterSql}"
        |> Sql.parameters (filters |> Seq.collect snd |> List.ofSeq)
        |> Sql.executeAsync Map.toListingForView


/// Success story data access functions
[<RequireQualifiedAccess>]
module Success =

    /// Find a success report by its ID
    let findById successId conn = backgroundTask {
        let! success =
            Sql.existingConnection conn
            |> Sql.query "SELECT * FROM jjj.success WHERE id = @id"
            |> Sql.parameters [ "@id", Sql.successId successId ]
            |> Sql.executeAsync Map.toSuccess
        return List.tryHead success
    }

    /// Insert or update a success story
    let save (success : Success) conn = backgroundTask {
        let! _ =
            Sql.existingConnection conn
            |> Sql.query
                "INSERT INTO jjj.success (
                    id, citizen_id, recorded_on, was_from_here, source, story
                ) VALUES (
                    @id, @citizenId, @recordedOn, @wasFromHere, @source, @story
                ) ON CONFLICT (id) DO UPDATE
                SET was_from_here = EXCLUDED.was_from_here,
                    story         = EXCLUDED.story"
            |> Sql.parameters
                [   "@id",          Sql.successId    success.id
                    "@citizenId",   Sql.citizenId    success.citizenId
                    "@recordedOn"   |>Sql.param<|    success.recordedOn
                    "@wasFromHere", Sql.bool         success.fromHere
                    "@source",      Sql.string       success.source
                    "@story",       Sql.stringOrNone (Option.map MarkdownString.toString success.story)
                ]
            |> Sql.executeNonQueryAsync
        ()
    }
    
    // Retrieve all success stories  
    let all conn =
        fromTable Table.Success
        |> eqJoin "citizenId" (fromTable Table.Citizen)
        |> without [ "right.id" ]
        |> zip
        |> mergeFunc (fun it -> {| citizenName = deriveDisplayName it; hasStory = it.G("story").Default_("").Gt "" |})
        |> pluck [ "id"; "citizenId"; "citizenName"; "recordedOn"; "fromHere"; "hasStory" ]
        |> orderByDescending "recordedOn"
        |> result<StoryEntry list> conn
