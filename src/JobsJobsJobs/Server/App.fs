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

open System.Text
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.Extensions.Configuration
open Microsoft.IdentityModel.Tokens
open NodaTime
open JobsJobsJobs.Data
open JobsJobsJobs.Domain.SharedTypes

/// Configure dependency injection
let configureServices (svc : IServiceCollection) =
    let _ = svc.AddGiraffe ()
    let _ = svc.AddSingleton<IClock> SystemClock.Instance
    let _ = svc.AddLogging ()
    let _ = svc.AddCors ()
    
    let _ = svc.AddSingleton<Json.ISerializer> (SystemTextJson.Serializer Json.options)

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
    
    // Set up the Marten data store
    match Connection.setUp cfg |> Async.AwaitTask |> Async.RunSynchronously with
    | Ok _ -> ()
    | Error msg -> failwith $"Error initializing data store: {msg}"


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
