
open Microsoft.Extensions.Configuration

/// Data access for v2 Jobs, Jobs, Jobs
module Rethink =

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

    open RethinkDb.Driver.Net

    /// Functions run at startup
    [<RequireQualifiedAccess>]
    module Startup =
      
        open NodaTime
        open NodaTime.Serialization.JsonNet
        open RethinkDb.Driver.FSharp
        
        /// Create a RethinkDB connection
        let createConnection (connStr : string) =
            // Add all required JSON converters
            Converter.Serializer.ConfigureForNodaTime DateTimeZoneProviders.Tzdb |> ignore
            // Connect to the database
            let config = DataConfig.FromUri connStr
            config.CreateConnection ()

/// Shorthand for the RethinkDB R variable (how every command starts)
let r = RethinkDb.Driver.RethinkDB.R

open JobsJobsJobs.Data
open JobsJobsJobs.Domain
open Newtonsoft.Json.Linq
open NodaTime
open NodaTime.Text
open RethinkDb.Driver.FSharp.Functions

/// Retrieve an instant from a JObject field
let getInstant (doc : JObject) name =
    let text = doc[name].Value<string> ()
    match InstantPattern.General.Parse text with
    | it when it.Success -> it.Value
    | _ ->
        match InstantPattern.ExtendedIso.Parse text with
        | it when it.Success -> it.Value
        | it -> raise it.Exception

task {
    // Establish database connections
    let cfg = ConfigurationBuilder().AddJsonFile("appsettings.json").Build ()
    use rethinkConn = Rethink.Startup.createConnection (cfg.GetConnectionString "RethinkDB")
    match! DataConnection.setUp cfg with
    | Ok _ -> ()
    | Error msg -> failwith msg
    
    // Migrate citizens
    let! oldCitizens =
        fromTable Rethink.Table.Citizen
        |> runResult<JObject list>
        |> withRetryOnce
        |> withConn rethinkConn
    let newCitizens =
        oldCitizens
        |> List.map (fun c ->
            let user = c["mastodonUser"].Value<string> ()
            { Citizen.empty with
                id         = CitizenId.ofString (c["id"].Value<string> ())
                joinedOn   = getInstant c "joinedOn"
                lastSeenOn = getInstant c "lastSeenOn"
                email      = $"""{user}@{c["instance"].Value<string> ()}"""
                firstName  = user
                lastName   = user
                isLegacy   = true
            })
    for citizen in newCitizens do
        do! Citizens.save citizen
    printfn $"** Migrated {List.length newCitizens} citizen(s)"
    ()
} |> Async.AwaitTask |> Async.RunSynchronously

