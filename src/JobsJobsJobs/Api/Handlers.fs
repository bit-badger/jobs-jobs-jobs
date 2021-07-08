/// Route handlers for Giraffe endpoints
module JobsJobsJobs.Api.Handlers

open FSharp.Control.Tasks
open Giraffe
open JobsJobsJobs.Domain
open JobsJobsJobs.Domain.SharedTypes
open JobsJobsJobs.Domain.Types

/// Helper functions
[<AutoOpen>]
module Helpers =

  open NodaTime
  open Microsoft.AspNetCore.Http
  open Microsoft.Extensions.Configuration
  open Microsoft.Extensions.Logging
  open RethinkDb.Driver.Net

  /// Get the NodaTime clock from the request context
  let clock (ctx : HttpContext) = ctx.GetService<IClock> ()

  /// Get the application configuration from the request context
  let config (ctx : HttpContext) = ctx.GetService<IConfiguration> ()

  /// Get the logger factory from the request context
  let logger (ctx : HttpContext) = ctx.GetService<ILoggerFactory> ()

  /// Get the RethinkDB connection from the request context
  let conn (ctx : HttpContext) = ctx.GetService<IConnection> ()

  /// Return None if the string is null, empty, or whitespace; otherwise, return Some and the trimmed string
  let noneIfEmpty x =
    match (defaultArg (Option.ofObj x) "").Trim () with | "" -> None | it -> Some it


/// Handlers for error conditions
module Error =

  /// Handler that will return a status code 404 and the text "Not Found"
  let notFound : HttpHandler =
    fun next ctx ->
      RequestErrors.NOT_FOUND $"The URL {string ctx.Request.Path} was not recognized as a valid URL" next ctx


/// Handler to return the files required for the Vue client app
module Vue =

  /// Handler that returns index.html (the Vue client app)
  let app : HttpHandler =
    fun next ctx ->
      match [ "GET"; "HEAD" ] |> List.contains ctx.Request.Method with
      | true -> htmlFile "wwwroot/index.html" next ctx
      | false -> Error.notFound next ctx


/// Handler for /api/citizen routes
module Citizen =

  // GET: /api/citizen/log-on/[code]
  let logOn authCode : HttpHandler =
    fun next ctx -> task {
      // Step 1 - Verify with Mastodon
      let cfg = (config ctx).GetSection "Auth"
      let log = (logger ctx).CreateLogger (nameof JobsJobsJobs.Api.Auth)
      
      match! Auth.verifyWithMastodon authCode cfg log with
      | Ok account ->
          // Step 2 - Find / establish Jobs, Jobs, Jobs account
          let  now     = (clock ctx).GetCurrentInstant ()
          let  dbConn  = conn ctx
          let! citizen = task {
            match! Data.Citizen.findByNaUser account.Username dbConn with
            | None ->
                let it : Citizen =
                  { id          = CitizenId.create ()
                    naUser      = account.Username
                    displayName = noneIfEmpty account.DisplayName
                    realName    = None
                    profileUrl  = account.Url
                    joinedOn    = now
                    lastSeenOn  = now
                    }
                do! Data.Citizen.add it dbConn
                return it
            | Some citizen ->
                let it = { citizen with displayName = noneIfEmpty account.DisplayName; lastSeenOn = now }
                do! Data.Citizen.logOnUpdate it dbConn
                return it
          }
          
          // Step 3 - Generate JWT
          return!
            json
              { jwt       = Auth.createJwt citizen cfg
                citizenId = CitizenId.toString citizen.id
                name      = Citizen.name citizen
                } next ctx
      | Error err ->
          return! RequestErrors.BAD_REQUEST err next ctx
      }
