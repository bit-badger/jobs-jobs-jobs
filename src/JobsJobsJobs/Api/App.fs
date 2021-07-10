/// The main API application for Jobs, Jobs, Jobs
module JobsJobsJobs.Api.App

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Giraffe
open Giraffe.EndpointRouting

/// All available routes for the application
let webApp = [
  subRoute "/api" [
    subRoute "/citizen" [
      GET [
        routef "/log-on/%s" Handlers.Citizen.logOn
        routef "/get/%O"    Handlers.Citizen.get
        ]
      DELETE [ route "" Handlers.Citizen.delete ]
      ]
    subRoute "/continent" [
      GET [ route "/all" Handlers.Continent.all ]
      ]
    subRoute "/profile" [
      GET [
        route  ""        Handlers.Profile.current
        route  "/count"  Handlers.Profile.count
        routef "/get/%O" Handlers.Profile.get
        ]
      POST [ route "/save" Handlers.Profile.save ]
      ]
    ]
  ]

/// Configure the ASP.NET Core pipeline to use Giraffe
let configureApp (app : IApplicationBuilder) =
  app
    .UseRouting()
    .UseEndpoints(fun e -> e.MapGiraffeEndpoints webApp)
  |> ignore

open NodaTime
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging

/// Configure dependency injection
let configureServices (svc : IServiceCollection) =
  svc.AddGiraffe ()                             |> ignore
  svc.AddSingleton<IClock> SystemClock.Instance |> ignore
  svc.AddLogging ()                             |> ignore
  let svcs = svc.BuildServiceProvider()
  let cfg  = svcs.GetRequiredService<IConfiguration>().GetSection "Rethink"
  let log  = svcs.GetRequiredService<ILoggerFactory>().CreateLogger (nameof Data.Startup)
  let conn = Data.Startup.createConnection cfg log
  svc.AddSingleton conn |> ignore
  Data.Startup.establishEnvironment cfg log conn |> Data.awaitIgnore

[<EntryPoint>]
let main _ =
  Host.CreateDefaultBuilder()
    .ConfigureWebHostDefaults(
      fun webHostBuilder ->
        webHostBuilder
          .Configure(configureApp)
          .ConfigureServices(configureServices)
        |> ignore)
    .Build()
    .Run ()
  0
