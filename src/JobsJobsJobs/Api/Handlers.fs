/// Route handlers for Giraffe endpoints
module JobsJobsJobs.Api.Handlers

open Giraffe
open JobsJobsJobs.Domain
open JobsJobsJobs.Domain.SharedTypes
open JobsJobsJobs.Domain.Types
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

/// Handler to return the files required for the Vue client app
module Vue =

  /// Handler that returns index.html (the Vue client app)
  let app = htmlFile "wwwroot/index.html"


/// Handlers for error conditions
module Error =
  
  open System.Threading.Tasks

  /// URL prefixes for the Vue app
  let vueUrls = [
    "/how-it-works"; "/privacy-policy"; "/terms-of-service"; "/citizen"; "/help-wanted"; "/listing"; "/profile"
    "/so-long"; "/success-story"
    ]

  /// Handler that will return a status code 404 and the text "Not Found"
  let notFound : HttpHandler =
    fun next ctx -> task {
      let fac  = ctx.GetService<ILoggerFactory> ()
      let log  = fac.CreateLogger "Handler"
      let path = string ctx.Request.Path
      match [ "GET"; "HEAD" ] |> List.contains ctx.Request.Method with
      | true when path = "/" || vueUrls |> List.exists path.StartsWith ->
          log.LogInformation "Returning Vue app"
          return! Vue.app next ctx
      | _ ->
          log.LogInformation "Returning 404"
          return! RequestErrors.NOT_FOUND $"The URL {path} was not recognized as a valid URL" next ctx
      }
  
  /// Handler that returns a 403 NOT AUTHORIZED response
  let notAuthorized : HttpHandler =
    setStatusCode 403 >=> fun _ _ -> Task.FromResult<HttpContext option> None

  /// Handler to log 500s and return a message we can display in the application
  let unexpectedError (ex: exn) (log : ILogger) =
    log.LogError(ex, "An unexpected error occurred")
    clearResponse >=> ServerErrors.INTERNAL_ERROR ex.Message
  

/// Helper functions
[<AutoOpen>]
module Helpers =

  open NodaTime
  open Microsoft.Extensions.Configuration
  open Microsoft.Extensions.Options
  open RethinkDb.Driver.Net
  open System.Security.Claims

  /// Get the NodaTime clock from the request context
  let clock (ctx : HttpContext) = ctx.GetService<IClock> ()

  /// Get the application configuration from the request context
  let config (ctx : HttpContext) = ctx.GetService<IConfiguration> ()

  /// Get the authorization configuration from the request context
  let authConfig (ctx : HttpContext) = (ctx.GetService<IOptions<AuthOptions>> ()).Value

  /// Get the logger factory from the request context
  let logger (ctx : HttpContext) = ctx.GetService<ILoggerFactory> ()

  /// Get the RethinkDB connection from the request context
  let conn (ctx : HttpContext) = ctx.GetService<IConnection> ()

  /// `None` if a `string option` is `None`, whitespace, or empty
  let noneIfBlank (s : string option) =
    s |> Option.map (fun x -> match x.Trim () with "" -> None | _ -> Some x) |> Option.flatten
  
  /// `None` if a `string` is null, empty, or whitespace; otherwise, `Some` and the trimmed string
  let noneIfEmpty = Option.ofObj >> noneIfBlank
  
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
  let currentCitizenId = tryUser >> Option.get >> CitizenId.ofString

  /// Return an empty OK response
  let ok : HttpHandler = Successful.OK ""



/// Handlers for /api/citizen routes
[<RequireQualifiedAccess>]
module Citizen =

  // GET: /api/citizen/log-on/[code]
  let logOn (abbr, authCode) : HttpHandler =
    fun next ctx -> task {
      // Step 1 - Verify with Mastodon
      let cfg = authConfig ctx
      
      match cfg.Instances |> Array.tryFind (fun it -> it.Abbr = abbr) with
      | Some instance ->
          let log = (logger ctx).CreateLogger (nameof JobsJobsJobs.Api.Auth)
          
          match! Auth.verifyWithMastodon authCode instance cfg.ReturnHost log with
          | Ok account ->
              // Step 2 - Find / establish Jobs, Jobs, Jobs account
              let  now     = (clock ctx).GetCurrentInstant ()
              let  dbConn  = conn ctx
              let! citizen = task {
                match! Data.Citizen.findByMastodonUser instance.Abbr account.Username dbConn with
                | None ->
                    let it : Citizen =
                      { id           = CitizenId.create ()
                        instance     = instance.Abbr
                        mastodonUser = account.Username
                        displayName  = noneIfEmpty account.DisplayName
                        realName     = None
                        profileUrl   = account.Url
                        joinedOn     = now
                        lastSeenOn   = now
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
          | Error err -> return! RequestErrors.BAD_REQUEST err next ctx
      | None -> return! Error.notFound next ctx
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
      return! ok next ctx
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


/// Handlers for /api/instances routes
[<RequireQualifiedAccess>]
module Instances =

  /// Convert a Masotodon instance to the one we use in the API
  let private toInstance (inst : MastodonInstance) =
    { name     = inst.Name
      url      = inst.Url
      abbr     = inst.Abbr
      clientId = inst.ClientId
      }

  // GET: /api/instances
  let all : HttpHandler =
    fun next ctx -> task {
      return! json ((authConfig ctx).Instances |> Array.map toInstance) next ctx
      }


/// Handlers for /api/listing[s] routes
[<RequireQualifiedAccess>]
module Listing =

  open NodaTime
  open System

  /// Parse the string we receive from JSON into a NodaTime local date
  let private parseDate = DateTime.Parse >> LocalDate.FromDateTime

  // GET: /api/listings/mine
  let mine : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      let! listings = Data.Listing.findByCitizen (currentCitizenId ctx) (conn ctx)
      return! json listings next ctx
      }

  // GET: /api/listing/[id]
  let get listingId : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      match! Data.Listing.findById (ListingId listingId) (conn ctx) with
      | Some listing -> return! json listing next ctx
      | None -> return! Error.notFound next ctx
      }
  
  // GET: /api/listing/view/[id]
  let view listingId : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      match! Data.Listing.findByIdForView (ListingId listingId) (conn ctx) with
      | Some listing -> return! json listing next ctx
      | None -> return! Error.notFound next ctx
      }

  // POST: /listings
  let add : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      let! form = ctx.BindJsonAsync<ListingForm> ()
      let  now  = (clock ctx).GetCurrentInstant ()
      do! Data.Listing.add
            { id            = ListingId.create ()
              citizenId     = currentCitizenId ctx
              createdOn     = now
              title         = form.title
              continentId   = ContinentId.ofString form.continentId
              region        = form.region
              remoteWork    = form.remoteWork
              isExpired     = false
              updatedOn     = now
              text          = Text form.text
              neededBy      = (form.neededBy |> Option.map parseDate)
              wasFilledHere = None
              } (conn ctx)
      return! ok next ctx
      }

  // PUT: /api/listing/[id]
  let update listingId : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      let dbConn = conn ctx
      match! Data.Listing.findById (ListingId listingId) dbConn with
      | Some listing when listing.citizenId <> (currentCitizenId ctx) -> return! Error.notAuthorized next ctx
      | Some listing ->
          let! form = ctx.BindJsonAsync<ListingForm> ()
          do! Data.Listing.update
                { listing with
                    title       = form.title
                    continentId = ContinentId.ofString form.continentId
                    region      = form.region
                    remoteWork  = form.remoteWork
                    text        = Text form.text
                    neededBy    = form.neededBy |> Option.map parseDate
                    updatedOn   = (clock ctx).GetCurrentInstant ()
                  } dbConn
          return! ok next ctx
      | None -> return! Error.notFound next ctx
      }
  
  // PATCH: /api/listing/[id]
  let expire listingId : HttpHandler =
    authorize
    >=> fun next ctx -> FSharp.Control.Tasks.Affine.task {
      let dbConn = conn ctx
      let now    = clock(ctx).GetCurrentInstant ()
      match! Data.Listing.findById (ListingId listingId) dbConn with
      | Some listing when listing.citizenId <> (currentCitizenId ctx) -> return! Error.notAuthorized next ctx
      | Some listing ->
          let! form = ctx.BindJsonAsync<ListingExpireForm> ()
          do! Data.Listing.expire listing.id form.fromHere now dbConn
          match form.successStory with
          | Some storyText ->
              do! Data.Success.save
                    { id         = SuccessId.create()
                      citizenId  = currentCitizenId ctx
                      recordedOn = now
                      fromHere   = form.fromHere
                      source     = "listing"
                      story      = (Text >> Some) storyText
                      } dbConn
          | None -> ()
          return! ok next ctx
      | None -> return! Error.notFound next ctx
      }

  // GET: /api/listing/search
  let search : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      let  search  = ctx.BindQueryString<ListingSearch> ()
      let! results = Data.Listing.search search (conn ctx)
      return! json results next ctx
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

  // GET: /api/profile/view/[id]
  let view citizenId : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      let citId  = CitizenId citizenId
      let dbConn = conn ctx
      match! Data.Profile.findById citId dbConn with
      | Some profile ->
          match! Data.Citizen.findById citId dbConn with
          | Some citizen ->
              match! Data.Continent.findById profile.continentId dbConn with
              | Some continent ->
                  return!
                    json {
                      profile   = profile
                      citizen   = citizen
                      continent = continent
                      } next ctx
              | None -> return! Error.notFound next ctx
          | None -> return! Error.notFound next ctx
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
                                        { id          = match s.id.StartsWith "new" with
                                                        | true -> SkillId.create ()
                                                        | false -> SkillId.ofString s.id
                                          description = s.description
                                          notes       = noneIfBlank s.notes
                                          })
              } dbConn
      do! Data.Citizen.realNameUpdate citizenId (noneIfBlank (Some form.realName)) dbConn
      return! ok next ctx
      }
  
  // PATCH: /api/profile/employment-found
  let employmentFound : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      let dbConn = conn ctx
      match! Data.Profile.findById (currentCitizenId ctx) dbConn with
      | Some profile ->
          do! Data.Profile.save { profile with seekingEmployment = false } dbConn
          return! ok next ctx
      | None -> return! Error.notFound next ctx
      }
  
  // DELETE: /api/profile
  let delete : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      do! Data.Profile.delete (currentCitizenId ctx) (conn ctx)
      return! ok next ctx
      }
  
  // GET: /api/profile/search
  let search : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      let  search  = ctx.BindQueryString<ProfileSearch> ()
      let! results = Data.Profile.search search (conn ctx)
      return! json results next ctx
      }
  
  // GET: /api/profile/public-search
  let publicSearch : HttpHandler =
    fun next ctx -> task {
      let  search  = ctx.BindQueryString<PublicSearch> ()
      let! results = Data.Profile.publicSearch search (conn ctx)
      return! json results next ctx
      }


/// Handlers for /api/success routes
[<RequireQualifiedAccess>]
module Success =

  open System

  // GET: /api/success/[id]
  let get successId : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      match! Data.Success.findById (SuccessId successId) (conn ctx) with
      | Some story -> return! json story next ctx
      | None -> return! Error.notFound next ctx
      }

  // GET: /api/success/list
  let all : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      let! stories = Data.Success.all (conn ctx)
      return! json stories next ctx
      }
  
  // POST: /api/success/save
  let save : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      let  citizenId = currentCitizenId ctx
      let  dbConn    = conn ctx
      let  now       = (clock ctx).GetCurrentInstant ()
      let! form      = ctx.BindJsonAsync<StoryForm> ()
      let! success = task {
        match form.id with
        | "new" ->
            return Some { id         = SuccessId.create ()
                          citizenId  = citizenId
                          recordedOn = now
                          fromHere   = form.fromHere
                          source     = "profile"
                          story      = noneIfEmpty form.story |> Option.map Text
                          }
        | successId ->
            match! Data.Success.findById (SuccessId.ofString successId) dbConn with
            | Some story when story.citizenId = citizenId ->
                return Some { story with
                                fromHere = form.fromHere
                                story    = noneIfEmpty form.story |> Option.map Text
                              }
            | Some _ | None -> return None
        }
      match success with
      | Some story ->
          do! Data.Success.save story dbConn
          return! ok next ctx
      | None -> return! Error.notFound next ctx
      }


open Giraffe.EndpointRouting

/// All available endpoints for the application
let allEndpoints = [
  subRoute "/api" [
    subRoute "/citizen" [
      GET_HEAD [
        routef "/log-on/%s/%s" Citizen.logOn
        routef "/%O"           Citizen.get
        ]
      DELETE [ route "" Citizen.delete ]
      ]
    GET_HEAD [ route "/continents" Continent.all ]
    GET_HEAD [ route "/instances"  Instances.all ]
    subRoute "/listing" [
      GET_HEAD [
        routef "/%O"      Listing.get
        route  "/search"  Listing.search
        routef "/%O/view" Listing.view
        route  "s/mine"   Listing.mine
        ]
      PATCH [
        routef "/%O" Listing.expire
        ]
      POST [
        route "s" Listing.add
        ]
      PUT [
        routef "/%O" Listing.update
        ]
      ]
    subRoute "/profile" [
      GET_HEAD [
        route  ""               Profile.current
        route  "/count"         Profile.count
        routef "/%O"            Profile.get
        routef "/%O/view"       Profile.view
        route  "/public-search" Profile.publicSearch
        route  "/search"        Profile.search
        ]
      PATCH [ route "/employment-found" Profile.employmentFound ]
      POST [ route "" Profile.save ]
      ]
    subRoute "/success" [
      GET_HEAD [
        routef "/%O" Success.get
        route  "es"  Success.all
        ]
      POST [ route "" Success.save ]
      ]
    ]
  ]
