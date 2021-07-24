/// The main API application for Jobs, Jobs, Jobs
module JobsJobsJobs.Api.App

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Giraffe
open Giraffe.EndpointRouting


/// Configure the ASP.NET Core pipeline to use Giraffe
let configureApp (app : IApplicationBuilder) =
  app
    .UseCors(fun p -> p.AllowAnyOrigin().AllowAnyHeader() |> ignore)
    .UseStaticFiles()
    .UseRouting()
    .UseEndpoints(fun e ->
        e.MapGiraffeEndpoints Handlers.allEndpoints
        e.MapFallbackToFile "index.html" |> ignore)
  |> ignore

open NodaTime
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging

/// Configure dependency injection
let configureServices (svc : IServiceCollection) =
  svc.AddGiraffe ()                             |> ignore
  svc.AddSingleton<IClock> SystemClock.Instance |> ignore
  svc.AddLogging ()                             |> ignore
  svc.AddCors ()                                |> ignore
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
