/// The main API application for Jobs, Jobs, Jobs
module JobsJobsJobs.Api.App

//open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Giraffe

/// All available routes for the application
let webApp =
  choose [
    route "/ping" >=> text "pong"
    route "/" >=> htmlFile "/pages/index.html"
    ]

/// Configure the ASP.NET Core pipeline to use Giraffe
let configureApp (app : IApplicationBuilder) =
  app.UseGiraffe webApp

open NodaTime
open RethinkDb.Driver.Net
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging

/// Configure dependency injection
let configureServices (svc : IServiceCollection) =
  svc.AddGiraffe()
    .AddSingleton<IClock>(SystemClock.Instance)
    .AddLogging ()
  |> ignore
  let svcs = svc.BuildServiceProvider()
  let cfg = svcs.GetRequiredService<IConfiguration>().GetSection "Rethink"
  let log = svcs.GetRequiredService<ILoggerFactory>().CreateLogger "Data.Startup"
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
