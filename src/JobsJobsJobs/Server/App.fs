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
    app.UseCors(fun p -> p.AllowAnyOrigin().AllowAnyHeader() |> ignore)
        .UseStaticFiles()
        .UseRouting()
        .UseAuthentication()
        .UseAuthorization()
        .UseGiraffeErrorHandler(Handlers.Error.unexpectedError)
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
open JobsJobsJobs.Data
open JobsJobsJobs.Domain.SharedTypes

/// Configure dependency injection
let configureServices (svc : IServiceCollection) =
    let _ = svc.AddGiraffe ()
    let _ = svc.AddSingleton<IClock> SystemClock.Instance
    let _ = svc.AddLogging ()
    let _ = svc.AddCors ()
    
    let jsonCfg = JsonSerializerSettings ()
    Data.Converters.all () |> List.iter jsonCfg.Converters.Add
    let _ = svc.AddSingleton<Json.ISerializer> (NewtonsoftJson.Serializer jsonCfg)

    let svcs = svc.BuildServiceProvider ()
    let cfg  = svcs.GetRequiredService<IConfiguration> ()
    
    // Set up JWTs for API access
    let _ =
        svc.AddAuthentication(fun o ->
            o.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
            o.DefaultChallengeScheme    <- JwtBearerDefaults.AuthenticationScheme
            o.DefaultScheme             <- JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun opt ->
                opt.RequireHttpsMetadata      <- false
                opt.TokenValidationParameters <- TokenValidationParameters (
                    ValidateIssuer   = true,
                    ValidateAudience = true,
                    ValidAudience    = "https://noagendacareers.com",
                    ValidIssuer      = "https://noagendacareers.com",
                    IssuerSigningKey = SymmetricSecurityKey (
                        Encoding.UTF8.GetBytes (cfg.GetSection "Auth").["ServerSecret"])))
    let _ = svc.AddAuthorization ()
    let _ = svc.Configure<AuthOptions> (cfg.GetSection "Auth")
    
    let dbCfg = cfg.GetSection "Rethink"
    let log   = svcs.GetRequiredService<ILoggerFactory>().CreateLogger "JobsJobsJobs.Api.Data.Startup"
    let conn  = Data.Startup.createConnection dbCfg log
    let _ = svc.AddSingleton conn |> ignore
    // Set up the Marten data store
    let _ = Connection.setUp cfg
    ()

[<EntryPoint>]
let main _ =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .Configure(configureApp)
                .ConfigureServices(configureServices)
            |> ignore)
        .Build()
        .Run ()
    0
