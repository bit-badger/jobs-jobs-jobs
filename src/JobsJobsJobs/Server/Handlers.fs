/// Route handlers for Giraffe endpoints
module JobsJobsJobs.Api.Handlers

open Giraffe
open JobsJobsJobs.Domain
open JobsJobsJobs.Domain.SharedTypes
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
    let vueUrls =
        [ "/how-it-works"; "/privacy-policy"; "/terms-of-service"; "/citizen"; "/help-wanted"; "/listing"; "/profile"
          "/so-long"; "/success-story"
        ]

    /// Handler that will return a status code 404 and the text "Not Found"
    let notFound : HttpHandler = fun next ctx -> task {
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

    open System.Security.Claims
    open NodaTime
    open Microsoft.Extensions.Configuration
    open Microsoft.Extensions.Options

    /// Get the NodaTime clock from the request context
    let now (ctx : HttpContext) = ctx.GetService<IClock>().GetCurrentInstant ()

    /// Get the application configuration from the request context
    let config (ctx : HttpContext) = ctx.GetService<IConfiguration> ()

    /// Get the authorization configuration from the request context
    let authConfig (ctx : HttpContext) = (ctx.GetService<IOptions<AuthOptions>> ()).Value

    /// Get the logger factory from the request context
    let logger (ctx : HttpContext) = ctx.GetService<ILoggerFactory> ()

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


open System
open JobsJobsJobs.Data

/// Handlers for /api/citizen routes
[<RequireQualifiedAccess>]
module Citizen =
    
    open Microsoft.AspNetCore.Identity

    // POST: /api/citizen/register
    let register : HttpHandler = fun next ctx -> task {
        let! form = ctx.BindJsonAsync<CitizenRegistrationForm> ()
        if form.Password.Length < 8 || form.ConfirmPassword.Length < 8 || form.Password <> form.ConfirmPassword then
            return! RequestErrors.BAD_REQUEST "Password out of range" next ctx
        else
            let now    = now ctx
            let noPass =
                { Citizen.empty with
                    Id          = CitizenId.create ()
                    Email       = form.Email
                    FirstName   = form.FirstName
                    LastName    = form.LastName
                    DisplayName = noneIfEmpty form.DisplayName
                    JoinedOn    = now
                    LastSeenOn  = now
                }
            let citizen = { noPass with PasswordHash = PasswordHasher().HashPassword (noPass, form.Password) }
            do! Citizens.save citizen
            // TODO: generate auth code and e-mail confirmation
            return! ok next ctx
    }
    
    // GET: /api/citizen/log-on/[code]
    let logOn (abbr, authCode) : HttpHandler = fun next ctx -> task {
        match! Citizens.tryLogOn "to@do.com" (fun _ -> false) (now ctx) with
        | Ok citizen ->
            return!
                json
                    { jwt       = Auth.createJwt citizen (authConfig ctx)
                      citizenId = CitizenId.toString citizen.Id
                      name      = Citizen.name citizen
                    } next ctx
        | Error msg ->
            // TODO: return error message
            return! RequestErrors.BAD_REQUEST msg next ctx
        // Step 1 - Verify with Mastodon
        // let cfg = authConfig ctx
        //
        // match cfg.Instances |> Array.tryFind (fun it -> it.Abbr = abbr) with
        // | Some instance ->
        //     let log = (logger ctx).CreateLogger (nameof JobsJobsJobs.Api.Auth)
        //     
        //     match! Auth.verifyWithMastodon authCode instance cfg.ReturnHost log with
        //     | Ok account ->
        //         // Step 2 - Find / establish Jobs, Jobs, Jobs account
        //         let  now     = (clock ctx).GetCurrentInstant ()
        //         let  dbConn  = conn ctx
        //         let! citizen = task {
        //             match! Data.Citizen.findByMastodonUser instance.Abbr account.Username dbConn with
        //             | None ->
        //                 let it : Citizen =
        //                     { id           = CitizenId.create ()
        //                       instance     = instance.Abbr
        //                       mastodonUser = account.Username
        //                       displayName  = noneIfEmpty account.DisplayName
        //                       realName     = None
        //                       profileUrl   = account.Url
        //                       joinedOn     = now
        //                       lastSeenOn   = now
        //                     }
        //                 do! Data.Citizen.add it dbConn
        //                 return it
        //             | Some citizen ->
        //                 let it = { citizen with displayName = noneIfEmpty account.DisplayName; lastSeenOn = now }
        //                 do! Data.Citizen.logOnUpdate it dbConn
        //                 return it
        //             }
        //         
        //         // Step 3 - Generate JWT
        //         return!
        //             json
        //                 { jwt       = Auth.createJwt citizen cfg
        //                   citizenId = CitizenId.toString citizen.id
        //                   name      = Citizen.name citizen
        //                 } next ctx
        //     | Error err -> return! RequestErrors.BAD_REQUEST err next ctx
        // | None -> return! Error.notFound next ctx
    }
    
    // GET: /api/citizen/[id]
    let get citizenId : HttpHandler = authorize >=> fun next ctx -> task {
        match! Citizens.findById (CitizenId citizenId) with
        | Some citizen -> return! json citizen next ctx
        | None -> return! Error.notFound next ctx
    }

    // DELETE: /api/citizen
    let delete : HttpHandler = authorize >=> fun next ctx -> task {
        do! Citizens.deleteById (currentCitizenId ctx)
        return! ok next ctx
    }


/// Handlers for /api/continent routes
[<RequireQualifiedAccess>]
module Continent =
  
    // GET: /api/continent/all
    let all : HttpHandler = fun next ctx -> task {
        let! continents = Continents.all ()
        return! json continents next ctx
    }


/// Handlers for /api/instances routes
[<RequireQualifiedAccess>]
module Instances =

    /// Convert a Mastodon instance to the one we use in the API
    let private toInstance (inst : MastodonInstance) =
        { name      = inst.Name
          url       = inst.Url
          abbr      = inst.Abbr
          clientId  = inst.ClientId
          isEnabled = inst.IsEnabled
          reason    = inst.Reason
        }

    // GET: /api/instances
    let all : HttpHandler = fun next ctx -> task {
        return! json ((authConfig ctx).Instances |> Array.map toInstance) next ctx
    }


/// Handlers for /api/listing[s] routes
[<RequireQualifiedAccess>]
module Listing =

    open NodaTime

    /// Parse the string we receive from JSON into a NodaTime local date
    let private parseDate = DateTime.Parse >> LocalDate.FromDateTime

    // GET: /api/listings/mine
    let mine : HttpHandler = authorize >=> fun next ctx -> task {
        let! listings = Listings.findByCitizen (currentCitizenId ctx)
        return! json listings next ctx
    }

    // GET: /api/listing/[id]
    let get listingId : HttpHandler = authorize >=> fun next ctx -> task {
        match! Listings.findById (ListingId listingId) with
        | Some listing -> return! json listing next ctx
        | None -> return! Error.notFound next ctx
    }
  
    // GET: /api/listing/view/[id]
    let view listingId : HttpHandler = authorize >=> fun next ctx -> task {
        match! Listings.findByIdForView (ListingId listingId) with
        | Some listing -> return! json listing next ctx
        | None -> return! Error.notFound next ctx
    }

    // POST: /listings
    let add : HttpHandler = authorize >=> fun next ctx -> task {
        let! form = ctx.BindJsonAsync<ListingForm> ()
        let  now  = now ctx
        do! Listings.save {
            Id            = ListingId.create ()
            CitizenId     = currentCitizenId ctx
            CreatedOn     = now
            Title         = form.title
            ContinentId   = ContinentId.ofString form.continentId
            Region        = form.region
            IsRemote      = form.remoteWork
            IsExpired     = false
            UpdatedOn     = now
            Text          = Text form.text
            NeededBy      = (form.neededBy |> Option.map parseDate)
            WasFilledHere = None
            IsLegacy      = false
        }
        return! ok next ctx
    }

    // PUT: /api/listing/[id]
    let update listingId : HttpHandler = authorize >=> fun next ctx -> task {
        match! Listings.findById (ListingId listingId) with
        | Some listing when listing.CitizenId <> (currentCitizenId ctx) -> return! Error.notAuthorized next ctx
        | Some listing ->
            let! form = ctx.BindJsonAsync<ListingForm> ()
            do! Listings.save
                    { listing with
                        Title       = form.title
                        ContinentId = ContinentId.ofString form.continentId
                        Region      = form.region
                        IsRemote    = form.remoteWork
                        Text        = Text form.text
                        NeededBy    = form.neededBy |> Option.map parseDate
                        UpdatedOn   = now ctx
                    }
            return! ok next ctx
        | None -> return! Error.notFound next ctx
      }
  
    // PATCH: /api/listing/[id]
    let expire listingId : HttpHandler = authorize >=> fun next ctx -> task {
        let now = now ctx
        match! Listings.findById (ListingId listingId) with
        | Some listing when listing.CitizenId <> (currentCitizenId ctx) -> return! Error.notAuthorized next ctx
        | Some listing ->
            let! form = ctx.BindJsonAsync<ListingExpireForm> ()
            do! Listings.save
                    { listing with
                        IsExpired     = true
                        WasFilledHere = Some form.fromHere
                        UpdatedOn     = now
                    }
            match form.successStory with
            | Some storyText ->
                do! Successes.save
                        { Id         = SuccessId.create()
                          CitizenId  = currentCitizenId ctx
                          RecordedOn = now
                          IsFromHere = form.fromHere
                          Source     = "listing"
                          Story      = (Text >> Some) storyText
                          }
            | None -> ()
            return! ok next ctx
        | None -> return! Error.notFound next ctx
    }

    // GET: /api/listing/search
    let search : HttpHandler = authorize >=> fun next ctx -> task {
        let  search  = ctx.BindQueryString<ListingSearch> ()
        let! results = Listings.search search
        return! json results next ctx
    }
  

/// Handlers for /api/profile routes
[<RequireQualifiedAccess>]
module Profile =

    // GET: /api/profile
    //      This returns the current citizen's profile, or a 204 if it is not found (a citizen not having a profile yet
    //      is not an error). The "get" handler returns a 404 if a profile is not found.
    let current : HttpHandler = authorize >=> fun next ctx -> task {
        match! Profiles.findById (currentCitizenId ctx) with
        | Some profile -> return! json profile next ctx
        | None -> return! Successful.NO_CONTENT next ctx
    }
  
    // GET: /api/profile/get/[id]
    let get citizenId : HttpHandler = authorize >=> fun next ctx -> task {
        match! Profiles.findById (CitizenId citizenId) with
        | Some profile -> return! json profile next ctx
        | None -> return! Error.notFound next ctx
    }

    // GET: /api/profile/view/[id]
    let view citizenId : HttpHandler = authorize >=> fun next ctx -> task {
        match! Profiles.findByIdForView (CitizenId citizenId) with
        | Some profile -> return! json profile next ctx
        | None -> return! Error.notFound next ctx
    }
  
    // GET: /api/profile/count
    let count : HttpHandler = authorize >=> fun next ctx -> task {
        let! theCount = Profiles.count ()
        return! json { count = theCount } next ctx
    }
  
    // POST: /api/profile/save
    let save : HttpHandler = authorize >=> fun next ctx -> task {
        let  citizenId = currentCitizenId ctx
        let! form      = ctx.BindJsonAsync<ProfileForm>()
        let! profile   = task {
            match! Profiles.findById citizenId with
            | Some p -> return p
            | None -> return { Profile.empty with Id = citizenId }
        }
        do! Profiles.save
                { profile with
                    IsSeekingEmployment  = form.isSeekingEmployment
                    IsPubliclySearchable = form.isPublic
                    ContinentId          = ContinentId.ofString form.continentId
                    Region               = form.region
                    IsRemote             = form.remoteWork
                    IsFullTime           = form.fullTime
                    Biography            = Text form.biography
                    LastUpdatedOn        = now ctx
                    Experience           = noneIfBlank form.experience |> Option.map Text
                    Skills               = form.skills
                                           |> List.map (fun s ->
                                               {   Id          = if s.id.StartsWith "new" then SkillId.create ()
                                                                 else SkillId.ofString s.id
                                                   Description = s.description
                                                   Notes       = noneIfBlank s.notes
                                               })
                }
        return! ok next ctx
    }
  
    // PATCH: /api/profile/employment-found
    let employmentFound : HttpHandler = authorize >=> fun next ctx -> task {
        match! Profiles.findById (currentCitizenId ctx) with
        | Some profile ->
            do! Profiles.save { profile with IsSeekingEmployment = false }
            return! ok next ctx
        | None -> return! Error.notFound next ctx
    }
  
    // DELETE: /api/profile
    let delete : HttpHandler = authorize >=> fun next ctx -> task {
        do! Profiles.deleteById (currentCitizenId ctx)
        return! ok next ctx
    }
  
    // GET: /api/profile/search
    let search : HttpHandler = authorize >=> fun next ctx -> task {
        let  search  = ctx.BindQueryString<ProfileSearch> ()
        let! results = Profiles.search search
        return! json results next ctx
    }
  
    // GET: /api/profile/public-search
    let publicSearch : HttpHandler = fun next ctx -> task {
        let  search  = ctx.BindQueryString<PublicSearch> ()
        let! results = Profiles.publicSearch search
        return! json results next ctx
    }


/// Handlers for /api/success routes
[<RequireQualifiedAccess>]
module Success =

    // GET: /api/success/[id]
    let get successId : HttpHandler = authorize >=> fun next ctx -> task {
        match! Successes.findById (SuccessId successId) with
        | Some story -> return! json story next ctx
        | None -> return! Error.notFound next ctx
    }

    // GET: /api/success/list
    let all : HttpHandler = authorize >=> fun next ctx -> task {
        let! stories = Successes.all ()
        return! json stories next ctx
    }
  
    // POST: /api/success/save
    let save : HttpHandler = authorize >=> fun next ctx -> task {
        let  citizenId = currentCitizenId ctx
        let! form      = ctx.BindJsonAsync<StoryForm> ()
        let! success = task {
            match form.id with
            | "new" ->
                return Some { Id         = SuccessId.create ()
                              CitizenId  = citizenId
                              RecordedOn = now ctx
                              IsFromHere = form.fromHere
                              Source     = "profile"
                              Story      = noneIfEmpty form.story |> Option.map Text
                              }
            | successId ->
                match! Successes.findById (SuccessId.ofString successId) with
                | Some story when story.CitizenId = citizenId ->
                    return Some { story with
                                    IsFromHere = form.fromHere
                                    Story    = noneIfEmpty form.story |> Option.map Text
                                }
                | Some _ | None -> return None
        }
        match success with
        | Some story ->
            do! Successes.save story
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
            POST [ route "/register" Citizen.register ]
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
            PATCH [ routef "/%O" Listing.expire ]
            POST [ route "s" Listing.add ]
            PUT [ routef "/%O" Listing.update ]
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
