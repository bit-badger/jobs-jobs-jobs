/// Data access functions for Jobs, Jobs, Jobs
module JobsJobsJobs.Api.Data

open JobsJobsJobs.Domain
open JobsJobsJobs.Domain.Types
open Polly
open RethinkDb.Driver
open RethinkDb.Driver.Net
open Microsoft.Extensions.Configuration
open FSharp.Control.Tasks
open Microsoft.Extensions.Logging

/// Shorthand for the RethinkDB R variable (how every command starts)
let private r = RethinkDB.R

/// Shorthand for await task / run sync / ignore (used in non-async contexts)
let awaitIgnore x = x |> Async.AwaitTask |> Async.RunSynchronously |> ignore


/// JSON converters used with RethinkDB persistence
module Converters =
  
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
  
  /// Create a RethinkDB connection
  let createConnection (cfg : IConfigurationSection) (log : ILogger)=
    
    // Add all required JSON converters
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
    do! ensureIndexes Table.Citizen [ "naUser" ]
    do! ensureIndexes Table.Listing [ "citizenId"; "continentId" ]
    do! ensureIndexes Table.Profile [ "continentId" ]
    do! ensureIndexes Table.Success [ "citizenId" ]
    }
