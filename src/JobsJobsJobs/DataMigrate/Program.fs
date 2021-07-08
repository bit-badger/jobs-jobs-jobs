// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open FSharp.Control.Tasks
open System
open System.Linq
open JobsJobsJobs.Api.Data
open JobsJobsJobs.Domain
open JobsJobsJobs.Server.Data
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open RethinkDb.Driver.Net

/// Create the host (reads configuration and initializes both databases)
let createHostBuilder argv =
  Host.CreateDefaultBuilder(argv)
    .ConfigureServices(
      Action<HostBuilderContext, IServiceCollection> (
        fun hostCtx svcs ->
            svcs.AddSingleton hostCtx.Configuration |> ignore
            // PostgreSQL via EF Core
            svcs.AddDbContext<JobsDbContext>(fun options ->
                options.UseNpgsql(hostCtx.Configuration.["ConnectionStrings:JobsDb"],
                  fun o -> o.UseNodaTime() |> ignore)
                |> ignore)
            |> ignore
            // RethinkDB
            let cfg  = hostCtx.Configuration.GetSection "Rethink"
            let log  = svcs.BuildServiceProvider().GetRequiredService<ILoggerFactory>().CreateLogger "Data"
            let conn = Startup.createConnection cfg log
            svcs.AddSingleton conn |> ignore
            Startup.establishEnvironment cfg log conn |> awaitIgnore
        ))
    .Build()

[<EntryPoint>]
let main argv =
  let host = createHostBuilder argv
  let r    = RethinkDb.Driver.RethinkDB.R
  
  printfn "0) Connecting to databases..."

  use db = host.Services.GetRequiredService<JobsDbContext> ()
  let conn = host.Services.GetRequiredService<IConnection> ()

  task {
    
    printfn "1) Migrating continents..."
    
    let mutable continentXref = Map.empty<string, Types.ContinentId>
    let! continents = db.Continents.AsNoTracking().ToListAsync ()
    let reContinents =
      continents
      |> Seq.map (fun c ->
          let reContinentId = ContinentId.create ()
          continentXref <- continentXref.Add (string c.Id, reContinentId)
          let it : Types.Continent = {
            id   = reContinentId
            name = c.Name
            }
          it)
      |> List.ofSeq
    let! _ = r.Table(Table.Continent).Insert(reContinents).RunWriteAsync conn

    printfn "2) Migrating citizens..."

    let mutable citizenXref = Map.empty<string, Types.CitizenId>
    let! citizens = db.Citizens.AsNoTracking().ToListAsync ()
    let reCitizens =
      citizens
      |> Seq.map (fun c ->
          let reCitizenId = CitizenId.create ()
          citizenXref <- citizenXref.Add (string c.Id, reCitizenId)
          let it : Types.Citizen = {
            id          = reCitizenId
            naUser      = c.NaUser
            displayName = Option.ofObj c.DisplayName
            realName    = Option.ofObj c.RealName
            profileUrl  = c.ProfileUrl
            joinedOn    = c.JoinedOn
            lastSeenOn  = c.LastSeenOn
            }
          it)
    let! _ = r.Table(Table.Citizen).Insert(reCitizens).RunWriteAsync conn

    printfn "3) Migrating profiles and skills..."

    let! profiles = db.Profiles.AsNoTracking().ToListAsync ()
    let reProfiles =
      profiles
      |> Seq.map (fun p ->
          let skills = db.Skills.AsNoTracking().Where(fun s -> s.CitizenId = p.Id).ToList ()
          let reSkills =
            skills
            |> Seq.map (fun skill ->
                let it : Types.Skill = {
                  id          = SkillId.create()
                  description = skill.Description
                  notes       = Option.ofObj skill.Notes
                  }
                it)
            |> List.ofSeq
          let it : Types.Profile = {
            id                = citizenXref.[string p.Id]
            seekingEmployment = p.SeekingEmployment
            isPublic          = p.IsPublic
            continentId       = continentXref.[string p.ContinentId]
            region            = p.Region
            remoteWork        = p.RemoteWork
            fullTime          = p.FullTime
            biography         = Types.Text p.Biography.Text
            lastUpdatedOn     = p.LastUpdatedOn
            experience        = match p.Experience with null -> None | x -> (Types.Text >> Some) x.Text
            skills            = reSkills
            }
          it)
    let! _ = r.Table(Table.Profile).Insert(reProfiles).RunWriteAsync conn

    printfn "4) Migrating success stories..."

    let! successes = db.Successes.AsNoTracking().ToListAsync ()
    let reSuccesses =
      successes
      |> Seq.map (fun s ->
          let it : Types.Success = {
              id         = SuccessId.create ()
              citizenId  = citizenXref.[string s.CitizenId]
              recordedOn = s.RecordedOn
              fromHere   = s.FromHere
              source     = "profile"
              story      = match s.Story with null -> None | x -> (Types.Text >> Some) x.Text
            }
          it)
    let! _ = r.Table(Table.Success).Insert(reSuccesses).RunWriteAsync conn
    ()
    }
  |> awaitIgnore

  printfn "Migration complete"

  0
