/// Route handlers for Giraffe endpoints
module JobsJobsJobs.Server.Handlers

open Giraffe
open JobsJobsJobs.Domain
open JobsJobsJobs.Domain.SharedTypes
open JobsJobsJobs.Views
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

/// Handler to return the files required for the Vue client app
module Vue =

    /// Handler that returns index.html (the Vue client app)
    let app = htmlFile "wwwroot/index.html"


open Giraffe.Htmx

[<AutoOpen>]
module private HtmxHelpers =
    
    /// Is the request from htmx?
    let isHtmx (ctx : HttpContext) =
        ctx.Request.IsHtmx && not ctx.Request.IsHtmxRefresh


/// Handlers for error conditions
module Error =
  
    open System.Net
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
    
    /// Handle unauthorized actions, redirecting to log on for GETs, otherwise returning a 401 Not Authorized response
    let notAuthorized : HttpHandler = fun next ctx ->
        if ctx.Request.Method = "GET" then
            let redirectUrl = $"/citizen/log-on?returnUrl={WebUtility.UrlEncode ctx.Request.Path}"
            if isHtmx ctx then (withHxRedirect redirectUrl >=> redirectTo false redirectUrl) next ctx
            else redirectTo false redirectUrl next ctx
        else
            if isHtmx ctx then
                (setHttpHeader "X-Toast" $"error|||You are not authorized to access the URL {ctx.Request.Path.Value}"
                 >=> setStatusCode 401) earlyReturn ctx
            else setStatusCode 401 earlyReturn ctx

    /// Handler to log 500s and return a message we can display in the application
    let unexpectedError (ex: exn) (log : ILogger) =
        log.LogError(ex, "An unexpected error occurred")
        clearResponse >=> ServerErrors.INTERNAL_ERROR ex.Message
  

open System
open NodaTime

/// Helper functions
[<AutoOpen>]
module Helpers =

    open System.Security.Claims
    open System.Text.Json
    open System.Text.RegularExpressions
    open Microsoft.AspNetCore.Antiforgery
    open Microsoft.Extensions.Configuration
    open Microsoft.Extensions.DependencyInjection
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

    // -- NEW --

    let antiForgery (ctx : HttpContext) =
        ctx.RequestServices.GetRequiredService<IAntiforgery> ()
    
    /// Obtain an anti-forgery token set
    let csrf ctx =
        (antiForgery ctx).GetAndStoreTokens ctx
    
    /// The key to use to indicate if we have loaded the session
    let private sessionLoadedKey = "session-loaded"

    /// Load the session if we have not yet
    let private loadSession (ctx : HttpContext) = task {
        if not (ctx.Items.ContainsKey sessionLoadedKey) then
            do! ctx.Session.LoadAsync ()
            ctx.Items.Add (sessionLoadedKey, "yes")
    }

    /// Save the session if we have loaded it
    let private saveSession (ctx : HttpContext) = task {
        if ctx.Items.ContainsKey sessionLoadedKey then do! ctx.Session.CommitAsync ()
    }

    /// Get the messages from the session (destructively)
    let popMessages ctx = task {
        do! loadSession ctx
        let msgs =
            match ctx.Session.GetString "messages" with
            | null -> []
            | m -> JsonSerializer.Deserialize<string list> m
        if not (List.isEmpty msgs) then ctx.Session.Remove "messages"
        return List.rev msgs
    }

    /// Add a message to the response
    let addMessage (level : string) (msg : string) ctx = task {
        do! loadSession ctx
        let! msgs = popMessages ctx
        ctx.Session.SetString ("messages", JsonSerializer.Serialize ($"{level}|||{msg}" :: msgs))
    }
    
    /// Add a success message to the response
    let addSuccess msg ctx = task {
        do! addMessage "success" msg ctx
    }
    
    /// Add an error message to the response
    let addError msg ctx = task {
        do! addMessage "error" msg ctx
    }

    /// Add a list of errors to the response
    let addErrors (errors : string list) ctx = task {
        let errMsg = String.Join ("</li><li>", errors)
        do! addError $"Please correct the following errors:<ul><li>{errMsg}</li></ul>" ctx
    }
    
    /// Render a page-level view
    let render pageTitle (_ : HttpFunc) (ctx : HttpContext) content = task {
        let! messages = popMessages ctx
        let  renderCtx : Layout.PageRenderContext = {
            IsLoggedOn = Option.isSome (tryUser ctx)
            CurrentUrl = ctx.Request.Path.Value
            PageTitle  = pageTitle
            Content    = content
            Messages   = messages
        }
        let renderFunc = if isHtmx ctx then Layout.partial else Layout.full
        return! ctx.WriteHtmlViewAsync (renderFunc renderCtx)
    }

    /// Render as a composable HttpHandler
    let renderHandler pageTitle content : HttpHandler = fun next ctx ->
        render pageTitle next ctx content

    /// Validate the anti cross-site request forgery token in the current request
    let validateCsrf : HttpHandler = fun next ctx -> task {
        match! (antiForgery ctx).IsRequestValidAsync ctx with
        | true -> return! next ctx
        | false -> return! RequestErrors.BAD_REQUEST "CSRF token invalid" earlyReturn ctx
    }

    /// Require a user to be logged on for a route
    let requireUser = requiresAuthentication Error.notAuthorized
    
    /// Regular expression to validate that a URL is a local URL
    let isLocal = Regex """^/[^\/\\].*"""

    /// Redirect to another page, saving the session before redirecting
    let redirectToGet (url : string) next ctx = task {
        do! saveSession ctx
        let action =
            if Option.isSome (noneIfEmpty url) && isLocal.IsMatch url then
                if isHtmx ctx then withHxRedirect url else redirectTo false url
            else RequestErrors.BAD_REQUEST "Invalid redirect URL"
        return! action next ctx
    }


open JobsJobsJobs.Data
open JobsJobsJobs.ViewModels


/// Handlers for /api routes
[<RequireQualifiedAccess>]
module Api =
    
    open System.IO

    // POST: /api/markdown-preview
    let markdownPreview : HttpHandler = requireUser >=> fun next ctx -> task {
        let _ = ctx.Request.Body.Seek(0L, SeekOrigin.Begin)
        use reader = new StreamReader (ctx.Request.Body)
        let! preview = reader.ReadToEndAsync ()
        return! htmlString (MarkdownString.toHtml (Text preview)) next ctx
    }


/// Handlers for /citizen routes
[<RequireQualifiedAccess>]
module Citizen =
    
    open Microsoft.AspNetCore.Authentication
    open Microsoft.AspNetCore.Authentication.Cookies
    open System.Security.Claims

    /// Support module for /citizen routes
    module private Support =
        
        /// The challenge questions and answers from the configuration
        let mutable private challenges : (string * string)[] option = None

        /// The challenge questions and answers
        let questions ctx =
            match challenges with
            | Some it -> it
            | None ->
                let qs = (config ctx).GetSection "ChallengeQuestions"
                let qAndA =
                    seq {
                        for idx in 0..4 do
                            let section = qs.GetSection(string idx)
                            yield section["Question"], (section["Answer"].ToLowerInvariant ())
                    }
                    |> Array.ofSeq
                challenges <- Some qAndA
                qAndA
        
    // GET: /citizen/confirm/[token]
    let confirm token next ctx = task {
        let! isConfirmed = Citizens.confirmAccount token
        return! Citizen.confirmAccount isConfirmed |> render "Account Confirmation" next ctx
    }

    // GET: /citizen/dashboard
    let dashboard = requireUser >=> fun next ctx -> task {
        let  citizenId = currentCitizenId ctx
        let! citizen   = Citizens.findById citizenId
        let! profile   = Profiles.findById citizenId
        let! prfCount  = Profiles.count ()
        return! Citizen.dashboard citizen.Value profile prfCount |> render "Dashboard" next ctx
    }

    // GET: /citizen/deny/[token]
    let deny token next ctx = task {
        let! wasDeleted = Citizens.denyAccount token
        return! Citizen.denyAccount wasDeleted |> render "Account Deletion" next ctx
    }

    // GET: /citizen/log-off
    let logOff = requireUser >=> fun next ctx -> task {
        do! ctx.SignOutAsync CookieAuthenticationDefaults.AuthenticationScheme
        do! addSuccess "Log off successful" ctx
        return! redirectToGet "/" next ctx
    }

    // GET: /citizen/log-on
    let logOn : HttpHandler = fun next ctx ->
        let returnTo =
            if ctx.Request.Query.ContainsKey "returnUrl" then Some ctx.Request.Query["returnUrl"].[0] else None
        Citizen.logOn { ErrorMessage = None; Email = ""; Password = ""; ReturnTo = returnTo } (csrf ctx)
        |> render "Log On" next ctx

    // POST: /citizen/log-on
    let doLogOn = validateCsrf >=> fun next ctx -> task {
        let! form = ctx.BindFormAsync<LogOnViewModel> ()
        match! Citizens.tryLogOn form.Email form.Password Auth.Passwords.verify Auth.Passwords.hash (now ctx) with
        | Ok citizen ->
            let claims = seq {
                Claim (ClaimTypes.NameIdentifier, CitizenId.toString citizen.Id)
                Claim (ClaimTypes.Name,           Citizen.name citizen)
            }
            let identity = ClaimsIdentity (claims, CookieAuthenticationDefaults.AuthenticationScheme)

            do! ctx.SignInAsync (identity.AuthenticationType, ClaimsPrincipal identity,
                AuthenticationProperties (IssuedUtc = DateTimeOffset.UtcNow))
            do! addSuccess "Log on successful" ctx
            return! redirectToGet (defaultArg form.ReturnTo "/citizen/dashboard") next ctx
        | Error msg ->
            do! addError msg ctx
            return! Citizen.logOn { form with Password = "" } (csrf ctx) |> render "Log On" next ctx
    }

    // GET: /citizen/register
    let register next ctx =
        // Get two different indexes for NA-knowledge challenge questions
        let q1Index = System.Random.Shared.Next(0, 5)
        let mutable q2Index = System.Random.Shared.Next(0, 5)
        while q1Index = q2Index do
            q2Index <- System.Random.Shared.Next(0, 5)
        let qAndA = Support.questions ctx
        Citizen.register (fst qAndA[q1Index]) (fst qAndA[q2Index])
            { RegisterViewModel.empty with Question1Index = q1Index; Question2Index = q2Index } (csrf ctx)
        |> render "Register" next ctx
    
    // POST: /citizen/register
    let doRegistration = validateCsrf >=> fun next ctx -> task {
        let! form  = ctx.BindFormAsync<RegisterViewModel> ()
        let  qAndA = Support.questions ctx
        let mutable badForm = false
        let errors = [
            if form.FirstName.Length < 1 then "First name is required"
            if form.LastName.Length  < 1 then "Last name is required"
            if form.Email.Length     < 1 then "E-mail address is required"
            if form.Password.Length  < 8 then "Password is too short"
            if   form.Question1Index < 0 || form.Question1Index > 4
              || form.Question2Index < 0 || form.Question2Index > 4
              || form.Question1Index = form.Question2Index then
                badForm <- true
            else if   (snd qAndA[form.Question1Index]) <> (form.Question1Answer.Trim().ToLowerInvariant ())
              || (snd qAndA[form.Question2Index]) <> (form.Question2Answer.Trim().ToLowerInvariant ()) then
                 "Question answers are incorrect"
        ]
        let refreshPage () =
            Citizen.register (fst qAndA[form.Question1Index]) (fst qAndA[form.Question2Index])
                    { form with Password = "" } (csrf ctx) |> renderHandler "Register"
                
        if badForm then
            do! addError "The form posted was invalid; please complete it again" ctx
            return! register next ctx
        else if List.isEmpty errors then
            let now    = now ctx
            let noPass =
                { Citizen.empty with
                    Id          = CitizenId.create ()
                    Email       = form.Email
                    FirstName   = form.FirstName
                    LastName    = form.LastName
                    DisplayName = noneIfBlank form.DisplayName
                    JoinedOn    = now
                    LastSeenOn  = now
                }
            let citizen = { noPass with PasswordHash = Auth.Passwords.hash noPass form.Password }
            let security =
                { SecurityInfo.empty with
                    Id            = citizen.Id
                    AccountLocked = true
                    Token         = Some (Auth.createToken citizen)
                    TokenUsage    = Some "confirm"
                    TokenExpires  = Some (now + (Duration.FromDays 3))
                }
            let! success = Citizens.register citizen security
            if success then
                let! emailResponse = Email.sendAccountConfirmation citizen security
                let  logFac        = logger ctx
                let  log           = logFac.CreateLogger "JobsJobsJobs.Handlers.Citizen"
                log.LogInformation $"Confirmation e-mail for {citizen.Email} received {emailResponse}"
                return! Citizen.registered |> render "Registration Successful" next ctx
            else
                do! addError "There is already an account registered to the e-mail address provided" ctx
                return! refreshPage () next ctx
        else
            do! addErrors errors ctx
            return! refreshPage () next ctx
    }


/// Handlers for /api/citizen routes
[<RequireQualifiedAccess>]
module CitizenApi =
    
    // GET: /api/citizen/[id]
    let get citizenId : HttpHandler = authorize >=> fun next ctx -> task {
        match! Citizens.findById (CitizenId citizenId) with
        | Some citizen -> return! json { citizen with PasswordHash = "" } next ctx
        | None -> return! Error.notFound next ctx
    }
    
    // PATCH: /api/citizen/account
    let account : HttpHandler = authorize >=> fun next ctx -> task {
        let! form = ctx.BindJsonAsync<AccountProfileForm> ()
        match! Citizens.findById (currentCitizenId ctx) with
        | Some citizen ->
            let password =
                if defaultArg form.NewPassword "" = "" then citizen.PasswordHash
                else Auth.Passwords.hash citizen form.NewPassword.Value
            do! Citizens.save
                    { citizen with
                        FirstName     = form.FirstName
                        LastName      = form.LastName
                        DisplayName   = noneIfBlank form.DisplayName
                        PasswordHash  = password
                        OtherContacts = form.Contacts
                                        |> List.map (fun c -> {
                                            Id          = if c.Id.StartsWith "new" then OtherContactId.create ()
                                                          else OtherContactId.ofString c.Id
                                            ContactType = ContactType.parse c.ContactType
                                            Name        = noneIfBlank c.Name
                                            Value       = c.Value
                                            IsPublic    = c.IsPublic
                                        })
                         }
            return! ok next ctx
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


/// Handlers for the home page, legal stuff, and help
[<RequireQualifiedAccess>]
module Home =

    // GET: /
    let home =
        renderHandler "Welcome" Home.home

    // GET: /how-it-works
    let howItWorks : HttpHandler =
        renderHandler "How It Works" Home.howItWorks
    
    // GET: /privacy-policy
    let privacyPolicy : HttpHandler =
        renderHandler "Privacy Policy" Home.privacyPolicy
    
    // GET: /terms-of-service
    let termsOfService : HttpHandler =
        renderHandler "Terms of Service" Home.termsOfService


/// Handlers for /api/listing[s] routes
[<RequireQualifiedAccess>]
module Listing =

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
            Title         = form.Title
            ContinentId   = ContinentId.ofString form.ContinentId
            Region        = form.Region
            IsRemote      = form.RemoteWork
            IsExpired     = false
            UpdatedOn     = now
            Text          = Text form.Text
            NeededBy      = (form.NeededBy |> Option.map parseDate)
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
                        Title       = form.Title
                        ContinentId = ContinentId.ofString form.ContinentId
                        Region      = form.Region
                        IsRemote    = form.RemoteWork
                        Text        = Text form.Text
                        NeededBy    = form.NeededBy |> Option.map parseDate
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
                        WasFilledHere = Some form.FromHere
                        UpdatedOn     = now
                    }
            match form.SuccessStory with
            | Some storyText ->
                do! Successes.save
                        { Id         = SuccessId.create()
                          CitizenId  = currentCitizenId ctx
                          RecordedOn = now
                          IsFromHere = form.FromHere
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
  

/// Handlers for /profile routes
[<RequireQualifiedAccess>]
module Profile =

    // GET: /profile/edit
    let edit : HttpHandler = requireUser >=> fun next ctx -> task {
        let! profile    = Profiles.findById (currentCitizenId ctx)
        let! continents = Continents.all ()
        let  isNew      = Option.isNone profile
        let  form       = if isNew then EditProfileViewModel.empty else EditProfileViewModel.fromProfile profile.Value
        let  title      = $"""{if isNew then "Create" else "Edit"} Profile"""
        return! Profile.edit form continents isNew (csrf ctx) |> render title next ctx
    }

    // POST: /profile/save
    let save : HttpHandler = requireUser >=> fun next ctx -> task {
        let  citizenId = currentCitizenId ctx
        let! theForm   = ctx.BindFormAsync<EditProfileViewModel> ()
        let  form      = { theForm with Skills = theForm.Skills |> Array.filter (box >> isNull >> not) }
        let  errors    = [
            if form.ContinentId = "" then "Continent is required"
            if form.Region      = "" then "Region is required"
            if form.Biography   = "" then "Professional Biography is required"
            if form.Skills |> Array.exists (fun s -> s.Description = "") then "All skill Descriptions are required"
        ]
        let! profile = task {
            match! Profiles.findById citizenId with
            | Some p -> return p
            | None -> return { Profile.empty with Id = citizenId }
        }
        let isNew = profile.Region = ""
        if List.isEmpty errors then
            do! Profiles.save
                    { profile with
                        IsSeekingEmployment  = form.IsSeekingEmployment
                        IsPubliclySearchable = form.IsPublic
                        ContinentId          = ContinentId.ofString form.ContinentId
                        Region               = form.Region
                        IsRemote             = form.RemoteWork
                        IsFullTime           = form.FullTime
                        Biography            = Text form.Biography
                        LastUpdatedOn        = now ctx
                        Experience           = noneIfBlank form.Experience |> Option.map Text
                        Skills               = form.Skills
                                               |> Array.filter (fun s -> (box >> isNull >> not) s)
                                               |> Array.map SkillForm.toSkill
                                               |> List.ofArray
                    }
            let action = if isNew then "cre" else "upd"
            do! addSuccess $"Employment Profile {action}ated successfully" ctx
            return! redirectToGet "/profile/edit" next ctx
        else
            do! addErrors errors ctx
            let! continents = Continents.all ()
            return!
                Profile.edit form continents isNew (csrf ctx)
                |> render $"""{if isNew then "Create" else "Edit"} Profile""" next ctx
    }

    // GET: /profile/[id]/view
    let view citizenId : HttpHandler = fun next ctx -> task {
        let citId = CitizenId.ofString citizenId
        match! Citizens.findById citId with
        | Some citizen ->
            match! Profiles.findById citId with
            | Some profile ->
                let currentCitizen = tryUser ctx |> Option.map CitizenId.ofString
                if not profile.IsPubliclyLinkable && Option.isNone currentCitizen then
                    return! Error.notAuthorized next ctx
                else
                    let! continent     = Continents.findById profile.ContinentId
                    let  continentName = match continent with Some c -> c.Name | None -> "not found"
                    let  title         = $"Employment Profile for {Citizen.name citizen}"
                    return!
                        Profile.view citizen profile continentName title currentCitizen
                        |> render title next ctx
            | None -> return! Error.notFound next ctx
        | None -> return! Error.notFound next ctx
    }
  

/// Handlers for /api/profile routes
[<RequireQualifiedAccess>]
module ProfileApi =

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
        return! json {| Count = theCount |} next ctx
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
            match form.Id with
            | "new" ->
                return Some { Id         = SuccessId.create ()
                              CitizenId  = citizenId
                              RecordedOn = now ctx
                              IsFromHere = form.FromHere
                              Source     = "profile"
                              Story      = noneIfEmpty form.Story |> Option.map Text
                              }
            | successId ->
                match! Successes.findById (SuccessId.ofString successId) with
                | Some story when story.CitizenId = citizenId ->
                    return Some { story with
                                    IsFromHere = form.FromHere
                                    Story    = noneIfEmpty form.Story |> Option.map Text
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
    GET_HEAD [ route "/" Home.home ]
    subRoute "/citizen" [
        GET_HEAD [
            routef "/confirm/%s" Citizen.confirm
            route  "/dashboard"  Citizen.dashboard
            routef "/deny/%s"    Citizen.deny
            route  "/log-off"    Citizen.logOff
            route  "/log-on"     Citizen.logOn
            route  "/register"   Citizen.register
        ]
        POST [
            route "/log-on"   Citizen.doLogOn
            route "/register" Citizen.doRegistration
        ]
    ]
    GET_HEAD [ route "/how-it-works" Home.howItWorks ]
    GET_HEAD [ route "/privacy-policy" Home.privacyPolicy ]
    subRoute "/profile" [
        GET_HEAD [
            routef "/%s/view" Profile.view
            route  "/edit"    Profile.edit
        ]
        POST [ route "/save" Profile.save ]
    ]
    GET_HEAD [ route "/terms-of-service" Home.termsOfService ]
    
    subRoute "/api" [
        subRoute "/citizen" [
            GET_HEAD [ routef "/%O" CitizenApi.get ]
            PATCH [
                route "/account" CitizenApi.account
            ]
            DELETE [
                route ""      CitizenApi.delete
            ]
        ]
        GET_HEAD [ route "/continents" Continent.all ]
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
        POST [ route "/markdown-preview" Api.markdownPreview ]
        subRoute "/profile" [
            GET_HEAD [
                route  ""               ProfileApi.current
                route  "/count"         ProfileApi.count
                routef "/%O"            ProfileApi.get
                routef "/%O/view"       ProfileApi.view
                route  "/public-search" ProfileApi.publicSearch
                route  "/search"        ProfileApi.search
            ]
            PATCH [ route "/employment-found" ProfileApi.employmentFound ]
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
