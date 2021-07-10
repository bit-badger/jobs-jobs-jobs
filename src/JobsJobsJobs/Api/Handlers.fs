/// Route handlers for Giraffe endpoints
module JobsJobsJobs.Api.Handlers

open FSharp.Control.Tasks
open Giraffe
open JobsJobsJobs.Domain
open JobsJobsJobs.Domain.SharedTypes
open JobsJobsJobs.Domain.Types
open Microsoft.AspNetCore.Http

/// Handler to return the files required for the Vue client app
module Vue =

  /// Handler that returns index.html (the Vue client app)
  let app = htmlFile "wwwroot/index.html"


/// Handlers for error conditions
module Error =
  
  open System.Threading.Tasks

  /// Handler that will return a status code 404 and the text "Not Found"
  let notFound : HttpHandler =
    fun next ctx -> task {
      match [ "GET"; "HEAD" ] |> List.contains ctx.Request.Method with
      | true ->
          // TODO: check for valid URL prefixes
          return! Vue.app next ctx
      | false ->
          return! RequestErrors.NOT_FOUND $"The URL {string ctx.Request.Path} was not recognized as a valid URL" next
                    ctx
      }
  
  /// Handler that returns a 403 NOT AUTHORIZED response
  let notAuthorized : HttpHandler =
    setStatusCode 403 >=> fun _ _ -> Task.FromResult<HttpContext option> None


/// Helper functions
[<AutoOpen>]
module Helpers =

  open NodaTime
  open Microsoft.Extensions.Configuration
  open Microsoft.Extensions.Logging
  open RethinkDb.Driver.Net
  open System.Security.Claims

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
  
  /// Return None if an optional string is None or empty
  let noneIfBlank s =
    s |> Option.map (fun x -> match x with "" -> None | _ -> Some x) |> Option.flatten
  
  /// Try to get the current user
  let tryUser (ctx : HttpContext) =
    ctx.User.FindFirst ClaimTypes.NameIdentifier
    |> Option.ofObj
    |> Option.map (fun x -> x.Value)

  /// Require a user to be logged in
  let authorize : HttpHandler =
    fun next ctx -> match tryUser ctx with Some _ -> next ctx | None -> Error.notAuthorized next ctx

  /// Get the ID of the currently logged in citizen
  //  NOTE: if no one is logged in, this will raise an exception
  let currentCitizenId ctx = (tryUser >> Option.get >> CitizenId.ofString) ctx



/// Handlers for /api/citizen routes
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
  
  // GET: /api/citizen/[id]
  let get citizenId : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      match! Data.Citizen.findById (CitizenId citizenId) (conn ctx) with
      | Some citizen -> return! json citizen next ctx
      | None -> return! Error.notFound next ctx
      }

  // DELETE: /api/citizen
  let delete : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      do! Data.Citizen.delete (currentCitizenId ctx) (conn ctx)
      return! Successful.OK "" next ctx
      }


/// Handlers for /api/continent routes
[<RequireQualifiedAccess>]
module Continent =
  
  // GET: /api/continent/all
  let all : HttpHandler =
    fun next ctx -> task {
      let! continents = Data.Continent.all (conn ctx)
      return! json continents next ctx
      }


/// Handlers for /api/profile routes
[<RequireQualifiedAccess>]
module Profile =

  // GET: /api/profile
  //      This returns the current citizen's profile, or a 204 if it is not found (a citizen not having a profile yet
  //      is not an error). The "get" handler returns a 404 if a profile is not found.
  let current : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      match! Data.Profile.findById (currentCitizenId ctx) (conn ctx) with
      | Some profile -> return! json profile next ctx
      | None -> return! Successful.NO_CONTENT next ctx
      }
  
  // GET: /api/profile/get/[id]
  let get citizenId : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      match! Data.Profile.findById (CitizenId citizenId) (conn ctx) with
      | Some profile -> return! json profile next ctx
      | None -> return! Error.notFound next ctx
      }
  
  // GET: /api/profile/count
  let count : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      let! theCount = Data.Profile.count (conn ctx)
      return! json { count = theCount } next ctx
    }
  
  // POST: /api/profile/save
  let save : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      let  citizenId = currentCitizenId ctx
      let  dbConn    = conn ctx
      let! form      = ctx.BindJsonAsync<ProfileForm>()
      let! profile   = task {
        match! Data.Profile.findById citizenId dbConn with
        | Some p -> return p
        | None -> return { Profile.empty with id = citizenId }
        }
      do! Data.Profile.save
            { profile with
                seekingEmployment = form.isSeekingEmployment
                isPublic          = form.isPublic
                continentId       = ContinentId.ofString form.continentId
                region            = form.region
                remoteWork        = form.remoteWork
                fullTime          = form.fullTime
                biography         = Text form.biography
                lastUpdatedOn     = (clock ctx).GetCurrentInstant ()
                experience        = noneIfBlank form.experience |> Option.map Text
                skills            = form.skills
                                    |> List.map (fun s ->
                                        { id          = SkillId.ofString s.id
                                          description = s.description
                                          notes       = noneIfBlank s.notes
                                          })
              } dbConn
      do! Data.Citizen.realNameUpdate citizenId (noneIfBlank (Some form.realName)) dbConn
      return! Successful.OK "" next ctx
    }
  