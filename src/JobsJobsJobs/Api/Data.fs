/// Data access functions for Jobs, Jobs, Jobs
module JobsJobsJobs.Api.Data

open FSharp.Control.Tasks
open JobsJobsJobs.Domain.Types
open Polly
open RethinkDb.Driver
open RethinkDb.Driver.Net
open RethinkDb.Driver.Ast

/// Shorthand for the RethinkDB R variable (how every command starts)
let private r = RethinkDB.R

/// Shorthand for await task / run sync / ignore (used in non-async contexts)
let awaitIgnore x = x |> Async.AwaitTask |> Async.RunSynchronously |> ignore


/// JSON converters used with RethinkDB persistence
module Converters =
  
  open JobsJobsJobs.Domain
  open Microsoft.FSharpLu.Json
  open Newtonsoft.Json
  open System

  /// JSON converter for citizen IDs
  type CitizenIdJsonConverter() =
    inherit JsonConverter<CitizenId>()
    override __.WriteJson(writer : JsonWriter, value : CitizenId, _ : JsonSerializer) =
      writer.WriteValue (CitizenId.toString value)
    override __.ReadJson(reader: JsonReader, _ : Type, _ : CitizenId, _ : bool, _ : JsonSerializer) =
      (string >> CitizenId.ofString) reader.Value
  
  /// JSON converter for continent IDs
  type ContinentIdJsonConverter() =
    inherit JsonConverter<ContinentId>()
    override __.WriteJson(writer : JsonWriter, value : ContinentId, _ : JsonSerializer) =
      writer.WriteValue (ContinentId.toString value)
    override __.ReadJson(reader: JsonReader, _ : Type, _ : ContinentId, _ : bool, _ : JsonSerializer) =
      (string >> ContinentId.ofString) reader.Value

  /// JSON converter for Markdown strings
  type MarkdownStringJsonConverter() =
    inherit JsonConverter<MarkdownString>()
    override __.WriteJson(writer : JsonWriter, value : MarkdownString, _ : JsonSerializer) =
      let (Text text) = value
      writer.WriteValue text
    override __.ReadJson(reader: JsonReader, _ : Type, _ : MarkdownString, _ : bool, _ : JsonSerializer) =
      (string >> Text) reader.Value

  /// JSON converter for listing IDs
  type ListingIdJsonConverter() =
    inherit JsonConverter<ListingId>()
    override __.WriteJson(writer : JsonWriter, value : ListingId, _ : JsonSerializer) =
      writer.WriteValue (ListingId.toString value)
    override __.ReadJson(reader: JsonReader, _ : Type, _ : ListingId, _ : bool, _ : JsonSerializer) =
      (string >> ListingId.ofString) reader.Value

  /// JSON converter for skill IDs
  type SkillIdJsonConverter() =
    inherit JsonConverter<SkillId>()
    override __.WriteJson(writer : JsonWriter, value : SkillId, _ : JsonSerializer) =
      writer.WriteValue (SkillId.toString value)
    override __.ReadJson(reader: JsonReader, _ : Type, _ : SkillId, _ : bool, _ : JsonSerializer) =
      (string >> SkillId.ofString) reader.Value
  
  /// JSON converter for success report IDs
  type SuccessIdJsonConverter() =
    inherit JsonConverter<SuccessId>()
    override __.WriteJson(writer : JsonWriter, value : SuccessId, _ : JsonSerializer) =
      writer.WriteValue (SuccessId.toString value)
    override __.ReadJson(reader: JsonReader, _ : Type, _ : SuccessId, _ : bool, _ : JsonSerializer) =
      (string >> SuccessId.ofString) reader.Value
  
  /// All JSON converters needed for the application
  let all () = [
    CitizenIdJsonConverter () :> JsonConverter
    upcast ContinentIdJsonConverter ()
    upcast MarkdownStringJsonConverter ()
    upcast ListingIdJsonConverter ()
    upcast SkillIdJsonConverter ()
    upcast SuccessIdJsonConverter ()
    upcast CompactUnionJsonConverter ()
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


/// Functions run at startup
[<RequireQualifiedAccess>]
module Startup =
  
  open Microsoft.Extensions.Configuration
  open Microsoft.Extensions.Logging
  open NodaTime
  open NodaTime.Serialization.JsonNet
  
  /// Create a RethinkDB connection
  let createConnection (cfg : IConfigurationSection) (log : ILogger) =
    
    // Add all required JSON converters
    Converter.Serializer.ConfigureForNodaTime DateTimeZoneProviders.Tzdb |> ignore
    Converters.all ()
    |> List.iter Converter.Serializer.Converters.Add
    // Read the configuration and create a connection
    let bldr =
      seq<Connection.Builder -> Connection.Builder> {
        yield fun b -> match cfg.["Hostname"] with null -> b | host -> b.Hostname host
        yield fun b -> match cfg.["Port"]     with null -> b | port -> (int >> b.Port) port
        yield fun b -> match cfg.["AuthKey"]  with null -> b | key  -> b.AuthKey key
        yield fun b -> match cfg.["Db"]       with null -> b | db   -> b.Db db
        yield fun b -> match cfg.["Timeout"]  with null -> b | time -> (int >> b.Timeout) time
        }
      |> Seq.fold (fun b step -> step b) (r.Connection ())
    match log.IsEnabled LogLevel.Debug with
    | true -> log.LogDebug $"RethinkDB: Connecting to {bldr.Hostname}:{bldr.Port}, database {bldr.Db}"
    | false -> ()
    bldr.Connect () :> IConnection

  /// Ensure the data, tables, and indexes that are required exist
  let establishEnvironment (cfg : IConfigurationSection) (log : ILogger) conn = task {
    // Ensure the database exists
    match cfg.["Db"] |> Option.ofObj with
    | Some database ->
        let! dbs = r.DbList().RunResultAsync<string list> conn
        match dbs |> List.contains database with
        | true -> ()
        | false ->
            log.LogInformation $"Creating database {database}..."
            let! _ = r.DbCreate(database).RunWriteAsync conn
            ()
    | None -> ()
    // Ensure the tables exist
    let! tables = r.TableList().RunResultAsync<string list> conn
    Table.all ()
    |> List.iter (
        fun tbl ->
            match tables |> List.contains tbl with
            | true -> ()
            | false ->
                log.LogInformation $"Creating {tbl} table..."
                r.TableCreate(tbl).RunWriteAsync conn |> awaitIgnore)
    // Ensure the indexes exist
    let ensureIndexes table indexes = task {
      let! tblIdxs = r.Table(table).IndexList().RunResultAsync<string list> conn
      indexes
      |> List.iter (
          fun idx ->
              match tblIdxs |> List.contains idx with
              | true -> ()
              | false ->
                  log.LogInformation $"Creating \"{idx}\" index on {table}"
                  r.Table(table).IndexCreate(idx).RunWriteAsync conn |> awaitIgnore)
      }
    do! ensureIndexes Table.Listing [ "citizenId"; "continentId"; "isExpired" ]
    do! ensureIndexes Table.Profile [ "continentId" ]
    do! ensureIndexes Table.Success [ "citizenId" ]
    // The instance/user is a compound index
    let! userIdx = r.Table(Table.Citizen).IndexList().RunResultAsync<string list> conn
    match userIdx |> List.contains "instanceUser" with
    | true -> ()
    | false ->
        let! _ =
          r.Table(Table.Citizen)
            .IndexCreate("instanceUser",
                ReqlFunction1 (fun row -> upcast r.Array (row.G "instance", row.G "mastodonUser")))
            .RunWriteAsync conn
        ()
    }


/// Determine if a record type (not nullable) is null
let toOption x = match x |> box |> isNull with true -> None | false -> Some x

[<AutoOpen>]
module private Reconnect =
  
  open System.Threading.Tasks

  /// Execute a query with a retry policy that will reconnect to RethinkDB if it has gone away
  let withReconn (conn : IConnection) (f : IConnection -> Task<'T>)  = 
    Policy
      .Handle<ReqlDriverError>()
      .RetryAsync(System.Action<exn, int> (fun ex _ ->
          printf "Encountered RethinkDB exception: %s" ex.Message
          match ex.Message.Contains "socket" with
          | true ->
              printf "Reconnecting to RethinkDB"
              (conn :?> Connection).Reconnect false
          | false -> ()))
      .ExecuteAsync(fun () -> f conn)

  /// Execute a query that returns one or none item, using the reconnect logic
  let withReconnOption (conn : IConnection) (f : IConnection -> Task<'T>) =
    fun c -> task {
      let! it = f c
      return toOption it
      }
    |> withReconn conn

  /// Execute a query that does not return a result, using the above reconnect logic
  let withReconnIgnore (conn : IConnection) (f : IConnection -> Task<'T>) =
    fun c -> task {
      let! _ = f c
      ()
      }
    |> withReconn conn

/// Sanitize user input, and create a "contains" pattern for use with RethinkDB queries
let regexContains = System.Text.RegularExpressions.Regex.Escape >> sprintf "(?i)%s"

open JobsJobsJobs.Domain
open JobsJobsJobs.Domain.SharedTypes

/// Profile data access functions
[<RequireQualifiedAccess>]
module Profile =

  let count conn =
    r.Table(Table.Profile)
      .Count()
      .RunResultAsync<int64>
    |> withReconn conn

  /// Find a profile by citizen ID
  let findById (citizenId : CitizenId) conn =
    r.Table(Table.Profile)
      .Get(citizenId)
      .RunResultAsync<Profile>
    |> withReconnOption conn
  
  /// Insert or update a profile
  let save (profile : Profile) conn =
    r.Table(Table.Profile)
      .Get(profile.id)
      .Replace(profile)
      .RunWriteAsync
    |> withReconnIgnore conn
  
  /// Delete a citizen's profile
  let delete (citizenId : CitizenId) conn =
    r.Table(Table.Profile)
      .Get(citizenId)
      .Delete()
      .RunWriteAsync
    |> withReconnIgnore conn
  
  /// Search profiles (logged-on users)
  let search (srch : ProfileSearch) conn =
    fun c ->
        (seq {
          match srch.continentId with
          | Some conId ->
              yield (fun (q : ReqlExpr) ->
                  q.Filter (r.HashMap (nameof srch.continentId, ContinentId.ofString conId)) :> ReqlExpr)
          | None -> ()
          match srch.remoteWork with
          | "" -> ()
          | _ -> yield (fun q -> q.Filter (r.HashMap (nameof srch.remoteWork, srch.remoteWork = "yes")) :> ReqlExpr)
          match srch.skill with
          | Some skl ->
              yield (fun q -> q.Filter (ReqlFunction1(fun it ->
                  upcast it.G("skills").Contains (ReqlFunction1(fun s ->
                      upcast s.G("description").Match (regexContains skl))))) :> ReqlExpr)
          | None -> ()
          match srch.bioExperience with
          | Some text ->
              let txt = regexContains text
              yield (fun q -> q.Filter (ReqlFunction1(fun it ->
                  upcast it.G("biography").Match(txt).Or (it.G("experience").Match txt))) :> ReqlExpr)
          | None -> ()
          }
        |> Seq.toList
        |> List.fold
            (fun q f -> f q)
            (r.Table(Table.Profile)
              .EqJoin("id", r.Table Table.Citizen)
              .Without(r.HashMap ("right", "id"))
              .Zip () :> ReqlExpr))
          .Merge(ReqlFunction1 (fun it ->
              upcast r
                .HashMap("displayName",
                  r.Branch (it.G("realName"   ).Default_("").Ne "", it.G "realName",
                            it.G("displayName").Default_("").Ne "", it.G "displayName",
                                                                    it.G "mastodonUser"))
                .With ("citizenId", it.G "id")))
          .Pluck("citizenId", "displayName", "seekingEmployment", "remoteWork", "fullTime", "lastUpdatedOn")
          .OrderBy(ReqlFunction1 (fun it -> upcast it.G("displayName").Downcase ()))
          .RunResultAsync<ProfileSearchResult list> c
    |> withReconn conn

  // Search profiles (public)
  let publicSearch (srch : PublicSearch) conn =
    fun c ->
        (seq {
          match srch.continentId with
          | Some conId ->
              yield (fun (q : ReqlExpr) ->
                  q.Filter (r.HashMap (nameof srch.continentId, ContinentId.ofString conId)) :> ReqlExpr)
          | None -> ()
          match srch.region with
          | Some reg ->
              yield (fun q ->
                  q.Filter (ReqlFunction1 (fun it -> upcast it.G("region").Match (regexContains reg))) :> ReqlExpr)
          | None -> ()
          match srch.remoteWork with
          | "" -> ()
          | _ -> yield (fun q -> q.Filter (r.HashMap (nameof srch.remoteWork, srch.remoteWork = "yes")) :> ReqlExpr)
          match srch.skill with
          | Some skl ->
              yield (fun q -> q.Filter (ReqlFunction1 (fun it ->
                  upcast it.G("skills").Contains (ReqlFunction1(fun s ->
                      upcast s.G("description").Match (regexContains skl))))) :> ReqlExpr)
          | None -> ()
          }
        |> Seq.toList
        |> List.fold
            (fun q f -> f q)
            (r.Table(Table.Profile)
              .EqJoin("continentId", r.Table Table.Continent)
              .Without(r.HashMap ("right", "id"))
              .Zip()
              .Filter(r.HashMap ("isPublic", true)) :> ReqlExpr))
          .Merge(ReqlFunction1 (fun it ->
              upcast r
                .HashMap("skills", 
                  it.G("skills").Map (ReqlFunction1 (fun skill ->
                    upcast r.Branch(skill.G("notes").Default_("").Eq "", skill.G "description",
                                    skill.G("description").Add(" (").Add(skill.G("notes")).Add ")"))))
                .With("continent", it.G "name")))
          .Pluck("continent", "region", "skills", "remoteWork")
          .RunResultAsync<PublicSearchResult list> c
    |> withReconn conn

/// Citizen data access functions
[<RequireQualifiedAccess>]
module Citizen =
  
  /// Find a citizen by their ID
  let findById (citizenId : CitizenId) conn =
    r.Table(Table.Citizen)
      .Get(citizenId)
      .RunResultAsync<Citizen>
    |> withReconnOption conn

  /// Find a citizen by their Mastodon username
  let findByMastodonUser (instance : string) (mastodonUser : string) conn =
    r.Table(Table.Citizen)
      .GetAll(r.Array (instance, mastodonUser)).OptArg("index", "instanceUser").Nth(0)
      .RunResultAsync<Citizen>
    |> withReconnOption conn
  
  /// Add a citizen
  let add (citizen : Citizen) conn =
    r.Table(Table.Citizen)
      .Insert(citizen)
      .RunWriteAsync
    |> withReconnIgnore conn

  /// Update the display name and last seen on date for a citizen
  let logOnUpdate (citizen : Citizen) conn =
    r.Table(Table.Citizen)
      .Get(citizen.id)
      .Update(r.HashMap( nameof citizen.displayName, citizen.displayName)
                  .With (nameof citizen.lastSeenOn,  citizen.lastSeenOn))
      .RunWriteAsync
    |> withReconnIgnore conn
  
  /// Delete a citizen
  let delete citizenId conn =
    fun c -> task {
        do! Profile.delete citizenId c
        let! _ =
          r.Table(Table.Success)
            .GetAll(citizenId).OptArg("index", "citizenId")
            .Delete()
            .RunWriteAsync c
        let! _ =
          r.Table(Table.Listing)
            .GetAll(citizenId).OptArg("index", "citizenId")
            .Delete()
            .RunWriteAsync c
        let! _ =
          r.Table(Table.Citizen)
            .Get(citizenId)
            .Delete()
            .RunWriteAsync c
        ()
      }
    |> withReconnIgnore conn
  
  /// Update a citizen's real name
  let realNameUpdate (citizenId : CitizenId) (realName : string option) conn =
    r.Table(Table.Citizen)
      .Get(citizenId)
      .Update(r.HashMap (nameof realName, realName))
      .RunWriteAsync
    |> withReconnIgnore conn


/// Continent data access functions
[<RequireQualifiedAccess>]
module Continent =

  /// Get all continents
  let all conn =
    r.Table(Table.Continent)
      .RunResultAsync<Continent list>
    |> withReconn conn
  
  /// Get a continent by its ID
  let findById (contId : ContinentId) conn =
    r.Table(Table.Continent)
      .Get(contId)
      .RunResultAsync<Continent>
    |> withReconnOption conn


/// Job listing data access functions
[<RequireQualifiedAccess>]
module Listing =  

  open NodaTime

  /// Find all job listings posted by the given citizen
  let findByCitizen (citizenId : CitizenId) conn =
    r.Table(Table.Listing)
      .GetAll(citizenId).OptArg("index", nameof citizenId)
      .EqJoin("continentId", r.Table Table.Continent)
      .Map(ReqlFunction1 (fun it -> upcast r.HashMap("listing", it.G "left").With ("continent", it.G "right")))
      .RunResultAsync<ListingForView list>
    |> withReconn conn
  
  /// Find a listing by its ID
  let findById (listingId : ListingId) conn =
    r.Table(Table.Listing)
      .Get(listingId)
      .RunResultAsync<Listing>
    |> withReconnOption conn
  
  /// Find a listing by its ID for viewing (includes continent information)
  let findByIdForView (listingId : ListingId) conn =
    fun c -> task {
        let! listing =
          r.Table(Table.Listing)
            .Filter(r.HashMap ("id", listingId))
            .EqJoin("continentId", r.Table Table.Continent)
            .Map(ReqlFunction1 (fun it -> upcast r.HashMap("listing", it.G "left").With ("continent", it.G "right")))
            .RunResultAsync<ListingForView list> c
        return List.tryHead listing
      }
    |> withReconn conn
  
  /// Add a listing
  let add (listing : Listing) conn =
    r.Table(Table.Listing)
      .Insert(listing)
      .RunWriteAsync
    |> withReconnIgnore conn
  
  /// Update a listing
  let update (listing : Listing) conn =
    r.Table(Table.Listing)
      .Get(listing.id)
      .Replace(listing)
      .RunWriteAsync
    |> withReconnIgnore conn

  /// Expire a listing
  let expire (listingId : ListingId) (fromHere : bool) (now : Instant) conn =
    r.Table(Table.Listing)
      .Get(listingId)
      .Update(r.HashMap("isExpired", true).With("wasFilledHere", fromHere).With ("updatedOn", now))
      .RunWriteAsync
    |> withReconnIgnore conn

  /// Search job listings
  let search (srch : ListingSearch) conn =
    fun c ->
        (seq {
          match srch.continentId with
          | Some conId ->
              yield (fun (q : ReqlExpr) ->
                  q.Filter (r.HashMap (nameof srch.continentId, ContinentId.ofString conId)) :> ReqlExpr)
          | None -> ()
          match srch.region with
          | Some rgn ->
              yield (fun q ->
                  q.Filter (ReqlFunction1 (fun it ->
                      upcast it.G(nameof srch.region).Match (regexContains rgn))) :> ReqlExpr)
          | None -> ()
          match srch.remoteWork with
          | "" -> ()
          | _ ->
              yield (fun q -> q.Filter (r.HashMap (nameof srch.remoteWork, srch.remoteWork = "yes")) :> ReqlExpr)
          match srch.text with
          | Some text ->
              yield (fun q ->
                  q.Filter (ReqlFunction1 (fun it ->
                      upcast it.G(nameof srch.text).Match (regexContains text))) :> ReqlExpr)
          | None -> ()
          }
        |> Seq.toList
        |> List.fold
            (fun q f -> f q)
            (r.Table(Table.Listing)
              .GetAll(false).OptArg ("index", "isExpired") :> ReqlExpr))
          .EqJoin("continentId", r.Table Table.Continent)
          .Map(ReqlFunction1 (fun it -> upcast r.HashMap("listing", it.G "left").With ("continent", it.G "right")))
          .RunResultAsync<ListingForView list> c
    |> withReconn conn


/// Success story data access functions
[<RequireQualifiedAccess>]
module Success =

  /// Find a success report by its ID
  let findById (successId : SuccessId) conn =
    r.Table(Table.Success)
      .Get(successId)
      .RunResultAsync<Success>
    |> withReconnOption conn

  /// Insert or update a success story
  let save (success : Success) conn =
    r.Table(Table.Success)
      .Get(success.id)
      .Replace(success)
      .RunWriteAsync
    |> withReconnIgnore conn

  // Retrieve all success stories  
  let all conn =
    r.Table(Table.Success)
      .EqJoin("citizenId", r.Table Table.Citizen)
      .Without(r.HashMap ("right", "id"))
      .Zip()
      .Merge(ReqlFunction1 (fun it ->
          upcast r
            .HashMap("citizenName",
              r.Branch(it.G("realName"   ).Default_("").Ne "", it.G "realName",
                       it.G("displayName").Default_("").Ne "", it.G "displayName",
                                                               it.G "mastodonUser"))
            .With ("hasStory", it.G("story").Default_("").Gt "")))
      .Pluck("id", "citizenId", "citizenName", "recordedOn", "fromHere", "hasStory")
      .OrderBy(r.Desc "recordedOn")
      .RunResultAsync<StoryEntry list>
    |> withReconn conn

(*
-- To replace naUser with mastodonUser for NAS --
r.db('jobsjobsjobs').table('citizen').replace(function(it) {
  return it.merge({ mastodonUser: it('naUser'), instance: 'nas' }).without('naUser');
})

*)
