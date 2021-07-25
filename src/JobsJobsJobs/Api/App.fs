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
    .UseAuthentication()
    .UseAuthorization()
    .UseEndpoints(fun e ->
        e.MapGiraffeEndpoints Handlers.allEndpoints
        e.MapFallbackToFile "index.html" |> ignore)
  |> ignore

open Newtonsoft.Json
open NodaTime
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Microsoft.IdentityModel.Tokens
open System.Text

/// Configure dependency injection
let configureServices (svc : IServiceCollection) =
  svc.AddGiraffe ()                             |> ignore
  svc.AddSingleton<IClock> SystemClock.Instance |> ignore
  svc.AddLogging ()                             |> ignore
  svc.AddCors ()                                |> ignore
  
  let jsonCfg = JsonSerializerSettings ()
  Data.Converters.all () |> List.iter jsonCfg.Converters.Add
  svc.AddSingleton<Json.ISerializer> (NewtonsoftJson.Serializer jsonCfg) |> ignore

  let svcs = svc.BuildServiceProvider ()
  let cfg  = svcs.GetRequiredService<IConfiguration> ()
  
  svc.AddAuthentication(fun o ->
      o.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
      o.DefaultChallengeScheme    <- JwtBearerDefaults.AuthenticationScheme
      o.DefaultScheme             <- JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(fun o ->
        o.RequireHttpsMetadata      <- false
        o.TokenValidationParameters <- TokenValidationParameters (
          ValidateIssuer   = true,
          ValidateAudience = true,
          ValidAudience    = "https://noagendacareers.com",
          ValidIssuer      = "https://noagendacareers.com",
          IssuerSigningKey = SymmetricSecurityKey (
            Encoding.UTF8.GetBytes (cfg.GetSection("Auth").["ServerSecret"]))))
    |> ignore
  svc.AddAuthorization () |> ignore

  let dbCfg = cfg.GetSection "Rethink"
  let log   = svcs.GetRequiredService<ILoggerFactory>().CreateLogger (nameof Data.Startup)
  let conn  = Data.Startup.createConnection dbCfg log
  svc.AddSingleton conn |> ignore
  Data.Startup.establishEnvironment dbCfg log conn |> Data.awaitIgnore

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
