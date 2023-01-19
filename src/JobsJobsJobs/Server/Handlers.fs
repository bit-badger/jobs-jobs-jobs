/// Route handlers for Giraffe endpoints
module JobsJobsJobs.Server.Handlers

open Giraffe
open Giraffe.Htmx
open JobsJobsJobs.Domain
open JobsJobsJobs.Domain.SharedTypes
open JobsJobsJobs.Views
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging


[<AutoOpen>]
module private HtmxHelpers =
    
    /// Is the request from htmx?
    let isHtmx (ctx : HttpContext) =
        ctx.Request.IsHtmx && not ctx.Request.IsHtmxRefresh


/// Handlers for error conditions
module Error =
  
    open System.Net

    /// Handler that will return a status code 404 and the text "Not Found"
    let notFound : HttpHandler = fun next ctx ->
        let fac  = ctx.GetService<ILoggerFactory> ()
        let log  = fac.CreateLogger "Handler"
        let path = string ctx.Request.Path
        log.LogInformation "Returning 404"
        RequestErrors.NOT_FOUND $"The URL {path} was not recognized as a valid URL" next ctx
    
    
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

    /// Get the NodaTime clock from the request context
    let now (ctx : HttpContext) = ctx.GetService<IClock>().GetCurrentInstant ()

    /// Get the application configuration from the request context
    let config (ctx : HttpContext) = ctx.GetService<IConfiguration> ()

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
    
    /// Get the time zone from the citizen's browser
    let timeZone (ctx : HttpContext) =
        let tz = string ctx.Request.Headers["X-Time-Zone"]
        defaultArg (noneIfEmpty tz) "Etc/UTC"

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
    
    // GET: /citizen/account
    let account : HttpHandler = fun next ctx -> task {
        match! Citizens.findById (currentCitizenId ctx) with
        | Some citizen ->
            return!
                Citizen.account (AccountProfileForm.fromCitizen citizen) (csrf ctx) |> render "Account Profile" next ctx
        | None -> return! Error.notFound next ctx
    }

    // GET: /citizen/cancel-reset/[token]
    let cancelReset token : HttpHandler = fun next ctx -> task {
        let! wasCanceled = task {
            match! Citizens.trySecurityByToken token with
            | Some security ->
                do! Citizens.saveSecurityInfo { security with Token = None; TokenUsage = None; TokenExpires = None }
                return true
            | None -> return false
        }
        return! Citizen.resetCanceled wasCanceled |> render "Password Reset Cancellation" next ctx
    }

    // GET: /citizen/confirm/[token]
    let confirm token : HttpHandler = fun next ctx -> task {
        let! isConfirmed = Citizens.confirmAccount token
        return! Citizen.confirmAccount isConfirmed |> render "Account Confirmation" next ctx
    }

    // GET: /citizen/dashboard
    let dashboard : HttpHandler = requireUser >=> fun next ctx -> task {
        let  citizenId = currentCitizenId ctx
        let! citizen   = Citizens.findById citizenId
        let! profile   = Profiles.findById citizenId
        let! prfCount  = Profiles.count ()
        return! Citizen.dashboard citizen.Value profile prfCount (timeZone ctx) |> render "Dashboard" next ctx
    }

    // POST: /citizen/delete
    let delete : HttpHandler = requireUser >=> validateCsrf >=> fun next ctx -> task {
        do! Citizens.deleteById (currentCitizenId ctx)
        do! ctx.SignOutAsync ()
        return! render "Account Deleted Successfully" next ctx Citizen.deleted
    }

    // GET: /citizen/deny/[token]
    let deny token : HttpHandler = fun next ctx -> task {
        let! wasDeleted = Citizens.denyAccount token
        return! Citizen.denyAccount wasDeleted |> render "Account Deletion" next ctx
    }

    // GET: /citizen/forgot-password
    let forgotPassword : HttpHandler = fun next ctx ->
        Citizen.forgotPassword (csrf ctx) |> render "Forgot Password" next ctx
    
    // POST: /citizen/forgot-password
    let doForgotPassword : HttpHandler = validateCsrf >=> fun next ctx -> task {
        let! form = ctx.BindFormAsync<ForgotPasswordForm> ()
        match! Citizens.tryByEmailWithSecurity form.Email with
        | Some (citizen, security) ->
            let withToken =
                { security with
                    Token         = Some (Auth.createToken citizen)
                    TokenUsage    = Some "reset"
                    TokenExpires  = Some (now ctx + (Duration.FromDays 3))
                }
            do! Citizens.saveSecurityInfo withToken
            let! emailResponse = Email.sendPasswordReset citizen withToken
            let  logFac        = logger ctx
            let  log           = logFac.CreateLogger "JobsJobsJobs.Handlers.Citizen"
            log.LogInformation $"Password reset e-mail for {citizen.Email} received {emailResponse}"
        | None -> ()
        // TODO: send link if it matches an account
        return! Citizen.forgotPasswordSent form |> render "Reset Request Processed" next ctx
    }

    // GET: /citizen/log-off
    let logOff : HttpHandler = requireUser >=> fun next ctx -> task {
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

    // GET: /citizen/reset-password/[token]
    let resetPassword token : HttpHandler = fun next ctx -> task {
        match! Citizens.trySecurityByToken token with
        | Some security ->
            return!
                Citizen.resetPassword { Id = CitizenId.toString security.Id; Token = token; Password = "" } (csrf ctx)
                |> render "Reset Password" next ctx
        | None -> return! Error.notFound next ctx
    }

    // POST: /citizen/reset-password
    let doResetPassword : HttpHandler = validateCsrf >=> fun next ctx -> task {
        let! form = ctx.BindFormAsync<ResetPasswordForm> ()
        let errors = [
            if form.Id    = "" then "Request invalid; please return to the link in your e-mail and try again"
            if form.Token = "" then "Request invalid; please return to the link in your e-mail and try again"
            if form.Password.Length < 8 then "Password too short"
        ]
        if List.isEmpty errors then
            match! Citizens.trySecurityByToken form.Token with
            | Some security when security.Id = CitizenId.ofString form.Id ->
                match! Citizens.findById security.Id with
                | Some citizen ->
                    do! Citizens.saveSecurityInfo { security with Token = None; TokenUsage = None; TokenExpires = None }
                    do! Citizens.save { citizen with PasswordHash = Auth.Passwords.hash citizen form.Password }
                    do! addSuccess "Password reset successfully; you may log on with your new credentials" ctx
                    return! redirectToGet "/citizen/log-on" next ctx
                | None -> return! Error.notFound next ctx
            | Some _
            | None -> return! Error.notFound next ctx
        else
            do! addErrors errors ctx 
            return! Citizen.resetPassword form (csrf ctx) |> render "Reset Password" next ctx
    }

    // POST: /citizen/save-account
    let saveAccount : HttpHandler = requireUser >=> validateCsrf >=> fun next ctx -> task {
        let! theForm   = ctx.BindFormAsync<AccountProfileForm> ()
        let  form      = { theForm with Contacts = theForm.Contacts |> Array.filter (box >> isNull >> not) }
        let  errors    = [
            if form.FirstName = "" then "First Name is required"
            if form.LastName  = "" then "Last Name is required"
            if form.NewPassword <> form.NewPassword then "New passwords do not match"
            if form.Contacts |> Array.exists (fun c -> c.ContactType = "") then "All Contact Types are required"
            if form.Contacts |> Array.exists (fun c -> c.Value = "") then "All Contacts are required"
        ]
        if List.isEmpty errors then
            match! Citizens.findById (currentCitizenId ctx) with
            | Some citizen ->
                let password =
                    if form.NewPassword = "" then citizen.PasswordHash
                    else Auth.Passwords.hash citizen form.NewPassword
                do! Citizens.save
                        { citizen with
                            FirstName     = form.FirstName
                            LastName      = form.LastName
                            DisplayName   = noneIfEmpty form.DisplayName
                            PasswordHash  = password
                            OtherContacts = form.Contacts
                                            |> Array.map (fun c ->
                                                {   OtherContact.Name = noneIfEmpty c.Name
                                                    ContactType       = ContactType.parse c.ContactType
                                                    Value             = c.Value
                                                    IsPublic          = c.IsPublic
                                                })
                                            |> List.ofArray
                            }
                let extraMsg = if form.NewPassword = "" then "" else " and password changed"
                do! addSuccess $"Account profile updated{extraMsg} successfully" ctx
                return! redirectToGet "/citizen/account" next ctx
            | None -> return! Error.notFound next ctx
        else
            do! addErrors errors ctx
            return! Citizen.account form (csrf ctx) |> render "Account Profile" next ctx
    }

    // GET: /citizen/so-long
    let soLong : HttpHandler = requireUser >=> fun next ctx ->
        Citizen.deletionOptions (csrf ctx) |> render "Account Deletion Options" next ctx


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


/// Handlers for /listing[s] routes (and /help-wanted)
[<RequireQualifiedAccess>]
module Listing =
    
    /// Parse the string we receive from JSON into a NodaTime local date
    let private parseDate = DateTime.Parse >> LocalDate.FromDateTime

    // GET: /listing/[id]/edit
    let edit listId : HttpHandler = requireUser >=> fun next ctx -> task {
        let citizenId = currentCitizenId ctx
        let! theListing = task {
            match listId with
            | "new" -> return Some { Listing.empty with CitizenId = citizenId }
            | _ -> return! Listings.findById (ListingId.ofString listId)
        }
        match theListing with
        | Some listing when listing.CitizenId = citizenId ->
            let! continents = Continents.all ()
            return!
                Listing.edit (EditListingForm.fromListing listing listId) continents (listId = "new") (csrf ctx)
                |> render $"""{if listId = "new" then "Add a" else "Edit"} Job Listing""" next ctx
        | Some _ -> return! Error.notAuthorized next ctx
        | None -> return! Error.notFound next ctx
    }

    // GET: /listing/[id]/expire
    let expire listingId : HttpHandler = requireUser >=> fun next ctx -> task {
        match! Listings.findById (ListingId listingId) with
        | Some listing when listing.CitizenId = currentCitizenId ctx ->
            if listing.IsExpired then
                do! addError $"The listing &ldquo;{listing.Title}&rdquo; is already expired" ctx
                return! redirectToGet "/listings/mine" next ctx
            else
                let form = { Id = ListingId.toString listing.Id; FromHere = false; SuccessStory = "" }
                return! Listing.expire form listing (csrf ctx) |> render "Expire Job Listing" next ctx
        | Some _ -> return! Error.notAuthorized next ctx
        | None -> return! Error.notFound next ctx
    }

    // POST: /listing/expire
    let doExpire : HttpHandler = requireUser >=> validateCsrf >=> fun next ctx -> task {
        let  citizenId = currentCitizenId ctx
        let  now       = now ctx
        let! form      = ctx.BindFormAsync<ExpireListingForm> ()
        match! Listings.findById (ListingId.ofString form.Id) with
        | Some listing when listing.CitizenId = citizenId ->
            if listing.IsExpired then
                return! RequestErrors.BAD_REQUEST "Request is already expired" next ctx
            else
                do! Listings.save
                        { listing with
                            IsExpired     = true
                            WasFilledHere = Some form.FromHere
                            UpdatedOn     = now
                        }
                if form.SuccessStory <> "" then
                    do! Successes.save
                            {   Id         = SuccessId.create()
                                CitizenId  = citizenId
                                RecordedOn = now
                                IsFromHere = form.FromHere
                                Source     = "listing"
                                Story      = (Text >> Some) form.SuccessStory
                            }
                let extraMsg = if form.SuccessStory <> "" then " and success story recorded" else ""
                do! addSuccess $"Job listing expired{extraMsg} successfully" ctx
                return! redirectToGet "/listings/mine" next ctx
        | Some _ -> return! Error.notAuthorized next ctx
        | None -> return! Error.notFound next ctx
    }

    // GET: /listings/mine
    let mine : HttpHandler = requireUser >=> fun next ctx -> task {
        let! listings = Listings.findByCitizen (currentCitizenId ctx)
        return! Listing.mine listings (timeZone ctx) |> render "My Job Listings" next ctx
    }

    // POST: /listing/save
    let save : HttpHandler = requireUser >=> validateCsrf >=> fun next ctx -> task {
        let  citizenId  = currentCitizenId ctx
        let  now        = now ctx
        let! form       = ctx.BindFormAsync<EditListingForm> ()
        let! theListing = task {
            match form.Id with
            | "new" ->
                return Some
                        { Listing.empty with
                            Id            = ListingId.create ()
                            CitizenId     = currentCitizenId ctx
                            CreatedOn     = now
                            IsExpired     = false
                            WasFilledHere = None
                            IsLegacy      = false
                        }
            | _ -> return! Listings.findById (ListingId.ofString form.Id)
        }
        match theListing with
        | Some listing when listing.CitizenId = citizenId ->
            do! Listings.save
                    { listing with
                        Title       = form.Title
                        ContinentId = ContinentId.ofString form.ContinentId
                        Region      = form.Region
                        IsRemote    = form.RemoteWork
                        Text        = Text form.Text
                        NeededBy    = noneIfEmpty form.NeededBy |> Option.map parseDate
                        UpdatedOn   = now
                    }
            do! addSuccess $"""Job listing {if form.Id = "new" then "add" else "updat"}ed successfully""" ctx
            return! redirectToGet $"/listing/{ListingId.toString listing.Id}/edit" next ctx
        | Some _ -> return! Error.notAuthorized next ctx
        | None -> return! Error.notFound next ctx

    }

    // GET: /help-wanted
    let search : HttpHandler = requireUser >=> fun next ctx -> task {
        let! continents = Continents.all ()
        let form =
            match ctx.TryBindQueryString<ListingSearchForm> () with
            | Ok f -> f
            | Error _ -> { ContinentId = ""; Region = ""; RemoteWork = ""; Text = "" }
        let! results = task {
            if string ctx.Request.Query["searched"] = "true" then
                let! it = Listings.search form
                return Some it
            else return None
        }
        return! Listing.search form continents results |> render "Help Wanted" next ctx
    }

    // GET: /listing/[id]/view
    let view listingId : HttpHandler = requireUser >=> fun next ctx -> task {
        match! Listings.findByIdForView (ListingId listingId) with
        | Some listing -> return! Listing.view listing |> render $"{listing.Listing.Title} | Job Listing" next ctx
        | None -> return! Error.notFound next ctx
    }


/// Handlers for /profile routes
[<RequireQualifiedAccess>]
module Profile =

    // POST: /profile/delete
    let delete : HttpHandler = requireUser >=> validateCsrf >=> fun next ctx -> task {
        do! Profiles.deleteById (currentCitizenId ctx)
        do! addSuccess "Profile deleted successfully" ctx
        return! redirectToGet "/citizen/dashboard" next ctx
    }

    // GET: /profile/edit
    let edit : HttpHandler = requireUser >=> fun next ctx -> task {
        let  citizenId  = currentCitizenId ctx
        let! profile    = Profiles.findById citizenId
        let! continents = Continents.all ()
        let  isNew      = Option.isNone profile
        let  form       = if isNew then EditProfileViewModel.empty else EditProfileViewModel.fromProfile profile.Value
        let  title      = $"""{if isNew then "Create" else "Edit"} Profile"""
        return! Profile.edit form continents isNew citizenId (csrf ctx) |> render title next ctx
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
                        ContinentId          = ContinentId.ofString form.ContinentId
                        Region               = form.Region
                        IsRemote             = form.RemoteWork
                        IsFullTime           = form.FullTime
                        Biography            = Text form.Biography
                        LastUpdatedOn        = now ctx
                        Skills               = form.Skills
                                               |> Array.filter (fun s -> (box >> isNull >> not) s)
                                               |> Array.map SkillForm.toSkill
                                               |> List.ofArray
                        Experience           = noneIfBlank form.Experience |> Option.map Text
                        IsPubliclySearchable = form.IsPubliclySearchable
                        IsPubliclyLinkable   = form.IsPubliclyLinkable
                    }
            let action = if isNew then "cre" else "upd"
            do! addSuccess $"Employment Profile {action}ated successfully" ctx
            return! redirectToGet "/profile/edit" next ctx
        else
            do! addErrors errors ctx
            let! continents = Continents.all ()
            return!
                Profile.edit form continents isNew citizenId (csrf ctx)
                |> render $"""{if isNew then "Create" else "Edit"} Profile""" next ctx
    }

    // GET: /profile/search
    let search : HttpHandler = requireUser >=> fun next ctx -> task {
        let! continents = Continents.all ()
        let form =
            match ctx.TryBindQueryString<ProfileSearchForm> () with
            | Ok f -> f
            | Error _ -> { ContinentId = ""; RemoteWork = ""; Skill = ""; BioExperience = "" }
        let! results = task {
            if string ctx.Request.Query["searched"] = "true" then
                let! it = Profiles.search form
                return Some it
            else return None
        }
        return! Profile.search form continents (timeZone ctx) results |> render "Profile Search" next ctx
    }

    // GET: /profile/seeking
    let seeking : HttpHandler = fun next ctx -> task {
        let! continents = Continents.all ()
        let form =
            match ctx.TryBindQueryString<PublicSearchForm> () with
            | Ok f -> f
            | Error _ -> { ContinentId = ""; Region = ""; RemoteWork = ""; Skill = "" }
        let! results = task {
            if string ctx.Request.Query["searched"] = "true" then
                let! it = Profiles.publicSearch form
                return Some it
            else return None
        }
        return! Profile.publicSearch form continents results |> render "Profile Search" next ctx
    }

    // GET: /profile/[id]/view
    let view citizenId : HttpHandler = fun next ctx -> task {
        let citId = CitizenId citizenId
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
                    return! Profile.view citizen profile continentName currentCitizen |> render title next ctx
            | None -> return! Error.notFound next ctx
        | None -> return! Error.notFound next ctx
    }
  

/// Handlers for /success-stor[y|ies] routes
[<RequireQualifiedAccess>]
module Success =

    // GET: /success-story/[id]/edit
    let edit successId : HttpHandler = requireUser >=> fun next ctx -> task {
        let  citizenId  = currentCitizenId ctx
        let  isNew      = successId = "new"
        let! theSuccess = task {
            if isNew then return Some { Success.empty with CitizenId = citizenId }
            else return! Successes.findById (SuccessId.ofString successId)
        }
        match theSuccess with
        | Some success when success.CitizenId = citizenId ->
            let pgTitle = $"""{if isNew then "Tell Your" else "Edit"} Success Story"""
            return!
                Success.edit (EditSuccessForm.fromSuccess success) (success.Id = SuccessId Guid.Empty) pgTitle
                             (csrf ctx)
                |> render pgTitle next ctx
        | Some _ -> return! Error.notAuthorized next ctx
        | None -> return! Error.notFound next ctx
    }

    // GET: /success-stories
    let list : HttpHandler = requireUser >=> fun next ctx -> task {
        let! stories = Successes.all ()
        return! Success.list stories (currentCitizenId ctx) (timeZone ctx) |> render "Success Stories" next ctx
    }
  
    // GET: /success-story/[id]/view
    let view successId : HttpHandler = requireUser >=> fun next ctx -> task {
        match! Successes.findById (SuccessId successId) with
        | Some success ->
            match! Citizens.findById success.CitizenId with
            | Some citizen ->
                return! Success.view success (Citizen.name citizen) (timeZone ctx) |> render "Success Story" next ctx
            | None -> return! Error.notFound next ctx
        | None -> return! Error.notFound next ctx
    }

    // POST: /success-story/save
    let save : HttpHandler = requireUser >=> validateCsrf >=> fun next ctx -> task {
        let  citizenId  = currentCitizenId ctx
        let! form       = ctx.BindFormAsync<EditSuccessForm> ()
        let  isNew      = form.Id = ShortGuid.fromGuid Guid.Empty
        let! theSuccess = task {
            if isNew then
                return Some
                        { Success.empty with
                            Id         = SuccessId.create ()
                            CitizenId  = citizenId
                            RecordedOn = now ctx
                            Source     = "profile"
                        }
            else return! Successes.findById (SuccessId.ofString form.Id)
        }
        match theSuccess with
        | Some story when story.CitizenId = citizenId ->
            do! Successes.save
                    { story with IsFromHere = form.FromHere; Story = noneIfEmpty form.Story |> Option.map Text }
            if isNew then
                match! Profiles.findById citizenId with
                | Some profile -> do! Profiles.save { profile with IsSeekingEmployment = false }
                | None -> ()
            let extraMsg = if isNew then " and seeking employment flag cleared" else ""
            do! addSuccess $"Success story saved{extraMsg} successfully" ctx
            return! redirectToGet "/success-stories" next ctx
        | Some _ -> return! Error.notAuthorized next ctx
        | None -> return! Error.notFound next ctx
    }


open Giraffe.EndpointRouting

/// All available endpoints for the application
let allEndpoints = [
    GET_HEAD [
        route "/"                 Home.home
        route "/help-wanted"      Listing.search
        route "/how-it-works"     Home.howItWorks
        route "/privacy-policy"   Home.privacyPolicy
        route "/terms-of-service" Home.termsOfService
    ]
    subRoute "/citizen" [
        GET_HEAD [
            route  "/account"           Citizen.account
            routef "/cancel-reset/%s"   Citizen.cancelReset
            routef "/confirm/%s"        Citizen.confirm
            route  "/dashboard"         Citizen.dashboard
            routef "/deny/%s"           Citizen.deny
            route  "/forgot-password"   Citizen.forgotPassword
            route  "/log-off"           Citizen.logOff
            route  "/log-on"            Citizen.logOn
            route  "/register"          Citizen.register
            routef "/reset-password/%s" Citizen.resetPassword
            route  "/so-long"           Citizen.soLong
        ]
        POST [
            route "/delete"          Citizen.delete
            route "/forgot-password" Citizen.doForgotPassword
            route "/log-on"          Citizen.doLogOn
            route "/register"        Citizen.doRegistration
            route "/reset-password"  Citizen.doResetPassword
            route "/save-account"    Citizen.saveAccount
        ]
    ]
    subRoute "/listing" [
        GET_HEAD [
            route  "s/mine"     Listing.mine
            routef "/%s/edit"   Listing.edit
            routef "/%O/expire" Listing.expire
            routef "/%O/view"   Listing.view
        ]
        POST [
            route "/expire" Listing.doExpire
            route "/save"   Listing.save
        ]
    ]
    subRoute "/profile" [
        GET_HEAD [
            routef "/%O/view" Profile.view
            route  "/edit"    Profile.edit
            route  "/search"  Profile.search
            route  "/seeking" Profile.seeking
        ]
        POST [
            route "/delete" Profile.delete
            route "/save"   Profile.save
        ]
    ]
    subRoute "/success-stor" [
        GET_HEAD [
            route  "ies"       Success.list
            routef "y/%s/edit" Success.edit
            routef "y/%O/view" Success.view
        ]
        POST [ route "y/save" Success.save ]
    ]
    subRoute "/api" [
        POST [ route "/markdown-preview" Api.markdownPreview ]
    ]
]
