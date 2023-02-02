
open System.Text.Json
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

open JobsJobsJobs
open JobsJobsJobs.Common.Data
open JobsJobsJobs.Domain
open Newtonsoft.Json.Linq
open NodaTime.Text
open Npgsql.FSharp
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
    let cfg = ConfigurationBuilder().AddJsonFile("appsettings.Migration.json").Build ()
    use rethinkConn = Rethink.Startup.createConnection (cfg.GetConnectionString "RethinkDB")
    do! setUp cfg
    let pgConn = dataSource ()
    
    let getOld table =
        fromTable table
        |> runResult<JObject list>
        |> withRetryOnce
        |> withConn rethinkConn
    
    // Migrate citizens
    let! oldCitizens = getOld Rethink.Table.Citizen
    let newCitizens =
        oldCitizens
        |> List.map (fun c ->
            let user = c["mastodonUser"].Value<string> ()
            { Citizen.empty with
                Id         = CitizenId.ofString (c["id"].Value<string> ())
                JoinedOn   = getInstant c "joinedOn"
                LastSeenOn = getInstant c "lastSeenOn"
                Email      = $"""{user}@{c["instance"].Value<string> ()}"""
                FirstName  = user
                LastName   = user
                IsLegacy   = true
            })
    for citizen in newCitizens do
        do! Citizens.Data.save citizen
    let! _ =
        pgConn
        |> Sql.executeTransactionAsync [
            $"INSERT INTO {Table.SecurityInfo} VALUES (@id, @data)",
            newCitizens |> List.map (fun c ->
                let info = { SecurityInfo.empty with Id = c.Id; AccountLocked = true }
                [   "@id",   Sql.string (CitizenId.toString c.Id)
                    "@data", Sql.jsonb (JsonSerializer.Serialize (info, Json.options))
                ])
        ]
    printfn $"** Migrated {List.length newCitizens} citizens"
    
    // Migrate continents
    let! oldContinents = getOld Rethink.Table.Continent
    let newContinents =
        oldContinents
        |> List.map (fun c ->
            { Continent.empty with
                Id   = ContinentId.ofString (c["id"].Value<string> ())
                Name = c["name"].Value<string> ()
            })
    let! _ =
        pgConn
        |> Sql.executeTransactionAsync [
            $"INSERT INTO {Table.Continent} VALUES (@id, @data)",
            newContinents |> List.map (fun c -> [
                "@id",   Sql.string (ContinentId.toString c.Id)
                "@data", Sql.jsonb (JsonSerializer.Serialize (c, Json.options))
            ])
        ]
    printfn $"** Migrated {List.length newContinents} continents"
    
    // Migrate profiles
    let! oldProfiles = getOld Rethink.Table.Profile
    let newProfiles =
        oldProfiles
        |> List.map (fun p ->
            let experience = p["experience"].Value<string> ()
            { Profile.empty with
                Id                  = CitizenId.ofString (p["id"].Value<string> ())
                ContinentId         = ContinentId.ofString (p["continentId"].Value<string> ())
                Region              = p["region"].Value<string> ()
                IsSeekingEmployment = p["seekingEmployment"].Value<bool> ()
                IsRemote            = p["remoteWork"].Value<bool> ()
                IsFullTime          = p["fullTime"].Value<bool> ()
                Biography           = Text (p["biography"].Value<string> ())
                Experience          = if isNull experience then None else Some (Text experience)
                Skills              = p["skills"].Children()
                                      |> Seq.map (fun s ->
                                           let notes = s["notes"].Value<string> ()
                                           {   Description = s["description"].Value<string> ()
                                               Notes       = if isNull notes then None else Some notes
                                           })
                                       |> List.ofSeq
                Visibility          = if p["isPublic"].Value<bool> () then Anonymous else Private
                LastUpdatedOn       = getInstant p "lastUpdatedOn"
                IsLegacy            = true
            })
    for profile in newProfiles do
        do! Profiles.Data.save profile
    printfn $"** Migrated {List.length newProfiles} profiles"
    
    // Migrate listings
    let! oldListings = getOld Rethink.Table.Listing
    let newListings =
        oldListings
        |> List.map (fun l ->
            let neededBy      = l["neededBy"].Value<string> ()
            let wasFilledHere = l["wasFilledHere"].Value<string> ()
            { Listing.empty with
                Id            = ListingId.ofString (l["id"].Value<string> ())
                CitizenId     = CitizenId.ofString (l["citizenId"].Value<string> ())
                CreatedOn     = getInstant l "createdOn"
                Title         = l["title"].Value<string> ()
                ContinentId   = ContinentId.ofString (l["continentId"].Value<string> ())
                Region        = l["region"].Value<string> ()
                IsRemote      = l["remoteWork"].Value<bool> ()
                IsExpired     = l["isExpired"].Value<bool> ()
                UpdatedOn     = getInstant l "updatedOn"
                Text          = Text (l["text"].Value<string> ())
                NeededBy      = if isNull neededBy then None else
                                match LocalDatePattern.Iso.Parse neededBy with
                                | it when it.Success -> Some it.Value
                                | it ->
                                    eprintfn $"Error parsing date - {it.Exception.Message}"
                                    None
                WasFilledHere = if isNull wasFilledHere then None else Some (bool.Parse wasFilledHere)
                IsLegacy      = true
            })
    for listing in newListings do
        do! Listings.Data.save listing
    printfn $"** Migrated {List.length newListings} listings"
    
    // Migrate success stories
    let! oldSuccesses = getOld Rethink.Table.Success
    let newSuccesses =
        oldSuccesses
        |> List.map (fun s ->
            let story = s["story"].Value<string> ()
            { Success.empty with
                Id         = SuccessId.ofString (s["id"].Value<string> ())
                CitizenId  = CitizenId.ofString (s["citizenId"].Value<string> ())
                RecordedOn = getInstant s "recordedOn"
                Source     = s["source"].Value<string> ()
                Story      = if isNull story then None else Some (Text story)
            })
    for success in newSuccesses do
        do! SuccessStories.Data.save success
    printfn $"** Migrated {List.length newSuccesses} successes"
    
    // Delete any citizens who have no profile, no listing, and no success story recorded
    let! deleted =
        pgConn
        |> Sql.query $"
            DELETE FROM {Table.Citizen}
             WHERE id NOT IN (SELECT id FROM {Table.Profile})
               AND id NOT IN (SELECT DISTINCT data ->> 'citizenId' FROM {Table.Listing})
               AND id NOT IN (SELECT DISTINCT data ->> 'citizenId' FROM {Table.Success})"
        |> Sql.executeNonQueryAsync
    printfn $"** Deleted {deleted} citizens who had no profile, listings, or success stories"
    
    printfn ""
    printfn "Migration complete"
} |> Async.AwaitTask |> Async.RunSynchronously

