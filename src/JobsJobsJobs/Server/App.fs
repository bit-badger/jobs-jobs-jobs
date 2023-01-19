/// The main web server application for Jobs, Jobs, Jobs
module JobsJobsJobs.Server.App

open System
open System.Text
open Giraffe
open Giraffe.EndpointRouting
open JobsJobsJobs.Data
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.HttpOverrides
open Microsoft.Extensions.Caching.Distributed
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open NodaTime


/// Enable buffering on the request body
type BufferedBodyMiddleware (next : RequestDelegate) =

    member _.InvokeAsync (ctx : HttpContext) = task {
        ctx.Request.EnableBuffering ()
        return! next.Invoke ctx
    }


[<EntryPoint>]
let main args =
    
    let builder = WebApplication.CreateBuilder args
    let svc     = builder.Services

    let _ = svc.AddGiraffe ()
    let _ = svc.AddSingleton<IClock> SystemClock.Instance
    let _ = svc.AddLogging ()
    let _ = svc.AddCors ()
    let _ = svc.AddSingleton<Json.ISerializer> (SystemTextJson.Serializer Json.options)
    let _ = svc.Configure<ForwardedHeadersOptions>(fun (opts : ForwardedHeadersOptions) ->
                opts.ForwardedHeaders <- ForwardedHeaders.XForwardedFor ||| ForwardedHeaders.XForwardedProto)
    let _ = svc.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(fun o ->
                    o.ExpireTimeSpan    <- TimeSpan.FromMinutes 60
                    o.SlidingExpiration <- true
                    o.AccessDeniedPath  <- "/error/not-authorized"
                    o.ClaimsIssuer      <- "https://noagendacareers.com")
    let _ = svc.AddAuthorization ()
    let _ = svc.AddAntiforgery ()
    
    // Set up the data store
    let cfg = svc.BuildServiceProvider().GetRequiredService<IConfiguration> ()
    let _ = DataConnection.setUp cfg |> Async.AwaitTask |> Async.RunSynchronously
    let _ = svc.AddSingleton<IDistributedCache> (fun _ -> DistributedCache () :> IDistributedCache)
    let _ = svc.AddSession(fun opts ->
                opts.IdleTimeout        <- TimeSpan.FromMinutes 60
                opts.Cookie.HttpOnly    <- true
                opts.Cookie.IsEssential <- true)
    
    let app = builder.Build ()

    let _ = app.UseForwardedHeaders ()
    let _ = app.UseCookiePolicy (CookiePolicyOptions (MinimumSameSitePolicy = SameSiteMode.Strict))
    let _ = app.UseStaticFiles ()
    let _ = app.UseRouting ()
    let _ = app.UseMiddleware<BufferedBodyMiddleware> ()
    let _ = app.UseAuthentication ()
    let _ = app.UseAuthorization ()
    let _ = app.UseSession ()
    let _ = app.UseGiraffeErrorHandler Handlers.Error.unexpectedError
    let _ = app.UseEndpoints (fun e -> e.MapGiraffeEndpoints Handlers.allEndpoints |> ignore)

    app.Run ()

    0
