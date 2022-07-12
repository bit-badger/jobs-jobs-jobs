/// Data access functions for Jobs, Jobs, Jobs
module JobsJobsJobs.Api.Data

open JobsJobsJobs.Domain.Types

/// JSON converters used with RethinkDB persistence
module Converters =
  
    open JobsJobsJobs.Domain
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
    
    /// The job listing table
    let Listing = "listing"
    
    /// The citizen employment profile table
    let Profile = "profile"
    
    /// The success story table
    let Success = "success"
    
    /// All tables
    let all () = [ Citizen; Continent; Listing; Profile; Success ]


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

/// Shorthand for the RethinkDB R variable (how every command starts)
let private r = RethinkDb.Driver.RethinkDB.R

/// Functions run at startup
[<RequireQualifiedAccess>]
module Startup =
  
    open Microsoft.Extensions.Configuration
    open Microsoft.Extensions.Logging
    open NodaTime
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

    /// Ensure the data, tables, and indexes that are required exist
    let establishEnvironment (cfg : IConfigurationSection) (log : ILogger) conn = task {
        // Ensure the database exists
        match cfg["database"] |> Option.ofObj with
        | Some database ->
            let! dbs = dbList () |> result<string list> conn
            match dbs |> List.contains database with
            | true -> ()
            | false ->
                log.LogInformation $"Creating database {database}..."
                do! dbCreate database |> write conn
                ()
        | None -> ()
        // Ensure the tables exist
        let! tables = tableListFromDefault () |> result<string list> conn
        for table in Table.all () do
            if not (List.contains table tables) then
                log.LogInformation $"Creating {table} table..."
                do! tableCreateInDefault table |> write conn
        // Ensure the indexes exist
        let ensureIndexes table indexes = task {
            let! tblIndexes = fromTable table |> indexList |> result<string list> conn
            for index in indexes do
                if not (List.contains index tblIndexes) then
                    log.LogInformation $"Creating \"{index}\" index on {table}"
                    do! fromTable table |> indexCreate index |> write conn
        }
        do! ensureIndexes Table.Listing [ "citizenId"; "continentId"; "isExpired" ]
        do! ensureIndexes Table.Profile [ "continentId" ]
        do! ensureIndexes Table.Success [ "citizenId" ]
        // The instance/user is a compound index
        let! userIdx = fromTable Table.Citizen |> indexList |> result<string list> conn
        if not (List.contains "instanceUser" userIdx) then
            do! fromTable Table.Citizen
                |> indexCreateFunc "instanceUser" (fun row -> [| row.G "instance"; row.G "mastodonUser" |])
                |> write conn
    }


open JobsJobsJobs.Domain
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

/// Profile data access functions
[<RequireQualifiedAccess>]
module Profile =

    /// Count the current profiles
    let count conn =
        fromTable Table.Profile
        |> count
        |> result<int64> conn

    /// Find a profile by citizen ID
    let findById (citizenId : CitizenId) conn =
        fromTable Table.Profile
        |> get citizenId
        |> resultOption<Profile> conn
  
    /// Insert or update a profile
    let save (profile : Profile) conn =
        fromTable Table.Profile
        |> get profile.id
        |> replace profile
        |> write conn
  
    /// Delete a citizen's profile
    let delete (citizenId : CitizenId) conn =
        fromTable Table.Profile
        |> get citizenId
        |> delete
        |> write conn
  
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
    let findById (citizenId : CitizenId) conn =
        fromTable Table.Citizen
        |> get citizenId
        |> resultOption<Citizen> conn

    /// Find a citizen by their Mastodon username
    let findByMastodonUser (instance : string) (mastodonUser : string) conn = task {
        let! u =
            fromTable Table.Citizen
            |> getAllWithIndex [ [| instance; mastodonUser |] ] "instanceUser"
            |> limit 1
            |> result<Citizen list> conn
        return List.tryHead u
    }
  
    /// Add a citizen
    let add (citizen : Citizen) conn =
        fromTable Table.Citizen
        |> insert citizen
        |> write conn

    /// Update the display name and last seen on date for a citizen
    let logOnUpdate (citizen : Citizen) conn =
        fromTable Table.Citizen
        |> get citizen.id
        |> update {| displayName = citizen.displayName; lastSeenOn = citizen.lastSeenOn |}
        |> write conn
  
    /// Delete a citizen
    let delete citizenId conn = task {
        do! Profile.delete citizenId conn
        do! fromTable Table.Success
            |> getAllWithIndex [ citizenId ] "citizenId"
            |> delete
            |> write conn
        do! fromTable Table.Listing
            |> getAllWithIndex [ citizenId ] "citizenId"
            |> delete
            |> write conn
        do! fromTable Table.Citizen
            |> get citizenId
            |> delete
            |> write conn
    }
    
    /// Update a citizen's real name
    let realNameUpdate (citizenId : CitizenId) (realName : string option) conn =
        fromTable Table.Citizen
        |> get citizenId
        |> update {| realName = realName |}
        |> write conn


/// Continent data access functions
[<RequireQualifiedAccess>]
module Continent =

    /// Get all continents
    let all conn =
        fromTable Table.Continent
        |> result<Continent list> conn
  
    /// Get a continent by its ID
    let findById (contId : ContinentId) conn =
        fromTable Table.Continent
        |> get contId
        |> resultOption<Continent> conn


/// Job listing data access functions
[<RequireQualifiedAccess>]
module Listing =  

    open NodaTime

    /// Convert a joined query to the form needed for ListingForView deserialization
    let private toListingForView (it : ReqlExpr) : obj = {| listing = it.G "left"; continent = it.G "right" |}
    
    /// Find all job listings posted by the given citizen
    let findByCitizen (citizenId : CitizenId) conn =
        fromTable Table.Listing
        |> getAllWithIndex [ citizenId ] (nameof citizenId)
        |> eqJoin "continentId" (fromTable Table.Continent)
        |> mapFunc toListingForView
        |> result<ListingForView list> conn
  
    /// Find a listing by its ID
    let findById (listingId : ListingId) conn =
        fromTable Table.Listing
        |> get listingId
        |> resultOption<Listing> conn
  
    /// Find a listing by its ID for viewing (includes continent information)
    let findByIdForView (listingId : ListingId) conn = task {
        let! listing =
            fromTable Table.Listing
            |> filter {| id = listingId |}
            |> eqJoin "continentId" (fromTable Table.Continent)
            |> mapFunc toListingForView
            |> result<ListingForView list> conn
        return List.tryHead listing
    }
  
    /// Add a listing
    let add (listing : Listing) conn =
        fromTable Table.Listing
        |> insert listing
        |> write conn
  
    /// Update a listing
    let update (listing : Listing) conn =
        fromTable Table.Listing
        |> get listing.id
        |> replace listing
        |> write conn

    /// Expire a listing
    let expire (listingId : ListingId) (fromHere : bool) (now : Instant) conn =
        (fromTable Table.Listing
         |> get listingId)
            .Update {| isExpired = true; wasFilledHere = fromHere; updatedOn = now |}
        |> write conn

    /// Search job listings
    let search (search : ListingSearch) conn =
        fromTable Table.Listing
        |> getAllWithIndex [ false ] "isExpired"
        |> applyFilters
            [ match search.continentId with
              | Some contId -> yield filter {| continentId = ContinentId.ofString contId |}
              | None -> ()
              match search.region with
              | Some rgn -> yield filterFunc (fun it -> it.G(nameof search.region).Match (regexContains rgn))
              | None -> ()
              match search.remoteWork with
              | "" -> ()
              | _ -> yield filter {| remoteWork = search.remoteWork = "yes" |}
              match search.text with
              | Some text -> yield filterFunc (fun it -> it.G(nameof search.text).Match (regexContains text))
              | None -> ()
            ]
        |> eqJoin "continentId" (fromTable Table.Continent)
        |> mapFunc toListingForView
        |> result<ListingForView list> conn


/// Success story data access functions
[<RequireQualifiedAccess>]
module Success =

    /// Find a success report by its ID
    let findById (successId : SuccessId) conn =
        fromTable Table.Success
        |> get successId
        |> resultOption conn

    /// Insert or update a success story
    let save (success : Success) conn =
        fromTable Table.Success
        |> get success.id
        |> replace success
        |> write conn

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
