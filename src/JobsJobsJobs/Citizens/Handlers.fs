module JobsJobsJobs.Citizens.Handlers

open System
open System.Security.Claims
open Giraffe
open JobsJobsJobs
open JobsJobsJobs.Citizens.Domain
open JobsJobsJobs.Common.Handlers
open JobsJobsJobs.Domain
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.Extensions.Logging
open NodaTime

/// Authorization functions
module private Auth =

    open System.Text

    /// Create a confirmation or password reset token for a user
    let createToken (citizen : Citizen) =
        Convert.ToBase64String (Guid.NewGuid().ToByteArray () |> Array.append (Encoding.UTF8.GetBytes citizen.Email))
    
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
    
    /// Password hashing and verification
    module Passwords =
        
        open Microsoft.AspNetCore.Identity

        /// The password hasher to use for the application
        let private hasher = PasswordHasher<Citizen> ()

        /// Hash a password for a user
        let hash citizen password =
            hasher.HashPassword (citizen, password)

        /// Verify a password (returns true if the password needs to be rehashed)
        let verify citizen password =
            match hasher.VerifyHashedPassword (citizen, citizen.PasswordHash, password) with
            | PasswordVerificationResult.Success -> Some false
            | PasswordVerificationResult.SuccessRehashNeeded -> Some true
            | _ -> None

    /// Require an administrative user (used for legacy migration endpoints)
    let requireAdmin : HttpHandler = requireUser >=> fun next ctx -> task {
        // let adminUser = (config ctx)["AdminUser"]
        // if adminUser = defaultArg (tryUser ctx) "" then return! next ctx
        // else return! Error.notAuthorized next ctx
        // TODO: uncomment the above, remove the line below
        return! next ctx
    }


// GET: /citizen/account
let account : HttpHandler = fun next ctx -> task {
    match! Data.findById (currentCitizenId ctx) with
    | Some citizen ->
        return!
            Views.account (AccountProfileForm.fromCitizen citizen) (isHtmx ctx) (csrf ctx)
            |> render "Account Profile" next ctx
    | None -> return! Error.notFound next ctx
}

// GET: /citizen/cancel-reset/[token]
let cancelReset token : HttpHandler = fun next ctx -> task {
    let! wasCanceled = task {
        match! Data.trySecurityByToken token with
        | Some security ->
            do! Data.saveSecurityInfo { security with Token = None; TokenUsage = None; TokenExpires = None }
            return true
        | None -> return false
    }
    return! Views.resetCanceled wasCanceled |> render "Password Reset Cancellation" next ctx
}

// GET: /citizen/confirm/[token]
let confirm token : HttpHandler = fun next ctx -> task {
    let! isConfirmed = Data.confirmAccount token
    return! Views.confirmAccount isConfirmed |> render "Account Confirmation" next ctx
}

// GET: /citizen/dashboard
let dashboard : HttpHandler = requireUser >=> fun next ctx -> task {
    let  citizenId = currentCitizenId ctx
    let! citizen   = Data.findById citizenId
    let! profile   = Profiles.Data.findById citizenId
    let! prfCount  = Profiles.Data.count ()
    return! Views.dashboard citizen.Value profile prfCount (timeZone ctx) |> render "Dashboard" next ctx
}

// POST: /citizen/delete
let delete : HttpHandler = requireUser >=> validateCsrf >=> fun next ctx -> task {
    do! Data.deleteById (currentCitizenId ctx)
    do! ctx.SignOutAsync ()
    return! render "Account Deleted Successfully" next ctx Views.deleted
}

// GET: /citizen/deny/[token]
let deny token : HttpHandler = fun next ctx -> task {
    let! wasDeleted = Data.denyAccount token
    return! Views.denyAccount wasDeleted |> render "Account Deletion" next ctx
}

// GET: /citizen/forgot-password
let forgotPassword : HttpHandler = fun next ctx ->
    Views.forgotPassword (csrf ctx) |> render "Forgot Password" next ctx

// POST: /citizen/forgot-password
let doForgotPassword : HttpHandler = validateCsrf >=> fun next ctx -> task {
    let! form = ctx.BindFormAsync<ForgotPasswordForm> ()
    match! Data.tryByEmailWithSecurity form.Email with
    | Some (citizen, security) ->
        let withToken =
            { security with
                Token         = Some (Auth.createToken citizen)
                TokenUsage    = Some "reset"
                TokenExpires  = Some (now ctx + (Duration.FromDays 3))
            }
        do! Data.saveSecurityInfo withToken
        let! emailResponse = Email.sendPasswordReset citizen withToken
        let  logFac        = logger ctx
        let  log           = logFac.CreateLogger "JobsJobsJobs.Handlers.Citizen"
        log.LogInformation $"Password reset e-mail for {citizen.Email} received {emailResponse}"
    | None -> ()
    return! Views.forgotPasswordSent form |> render "Reset Request Processed" next ctx
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
    Views.logOn { ErrorMessage = None; Email = ""; Password = ""; ReturnTo = returnTo } (csrf ctx)
    |> render "Log On" next ctx


// POST: /citizen/log-on
let doLogOn : HttpHandler = validateCsrf >=> fun next ctx -> task {
    let! form = ctx.BindFormAsync<LogOnForm> ()
    match! Data.tryLogOn form.Email form.Password Auth.Passwords.verify Auth.Passwords.hash (now ctx) with
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
        return! Views.logOn { form with Password = "" } (csrf ctx) |> render "Log On" next ctx
}

// GET: /citizen/register
let register next ctx =
    // Get two different indexes for NA-knowledge challenge questions
    let q1Index = System.Random.Shared.Next(0, 5)
    let mutable q2Index = System.Random.Shared.Next(0, 5)
    while q1Index = q2Index do
        q2Index <- System.Random.Shared.Next(0, 5)
    let qAndA = Auth.questions ctx
    Views.register (fst qAndA[q1Index]) (fst qAndA[q2Index])
        { RegisterForm.empty with Question1Index = q1Index; Question2Index = q2Index } (isHtmx ctx) (csrf ctx)
    |> render "Register" next ctx

// POST: /citizen/register
let doRegistration : HttpHandler = validateCsrf >=> fun next ctx -> task {
    let! form  = ctx.BindFormAsync<RegisterForm> ()
    let  qAndA = Auth.questions ctx
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
        Views.register (fst qAndA[form.Question1Index]) (fst qAndA[form.Question2Index]) { form with Password = "" }
                       (isHtmx ctx) (csrf ctx)
        |> renderHandler "Register"
            
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
        let! success = Data.register citizen security
        if success then
            let! emailResponse = Email.sendAccountConfirmation citizen security
            let  logFac        = logger ctx
            let  log           = logFac.CreateLogger "JobsJobsJobs.Handlers.Citizen"
            log.LogInformation $"Confirmation e-mail for {citizen.Email} received {emailResponse}"
            return! Views.registered |> render "Registration Successful" next ctx
        else
            do! addError "There is already an account registered to the e-mail address provided" ctx
            return! refreshPage () next ctx
    else
        do! addErrors errors ctx
        return! refreshPage () next ctx
}

// GET: /citizen/reset-password/[token]
let resetPassword token : HttpHandler = fun next ctx -> task {
    match! Data.trySecurityByToken token with
    | Some security ->
        return!
            Views.resetPassword { Id = CitizenId.toString security.Id; Token = token; Password = "" } (isHtmx ctx)
                                (csrf ctx)
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
        match! Data.trySecurityByToken form.Token with
        | Some security when security.Id = CitizenId.ofString form.Id ->
            match! Data.findById security.Id with
            | Some citizen ->
                do! Data.saveSecurityInfo { security with Token = None; TokenUsage = None; TokenExpires = None }
                do! Data.save { citizen with PasswordHash = Auth.Passwords.hash citizen form.Password }
                do! addSuccess "Password reset successfully; you may log on with your new credentials" ctx
                return! redirectToGet "/citizen/log-on" next ctx
            | None -> return! Error.notFound next ctx
        | Some _
        | None -> return! Error.notFound next ctx
    else
        do! addErrors errors ctx 
        return! Views.resetPassword form (isHtmx ctx) (csrf ctx) |> render "Reset Password" next ctx
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
        match! Data.findById (currentCitizenId ctx) with
        | Some citizen ->
            let password =
                if form.NewPassword = "" then citizen.PasswordHash
                else Auth.Passwords.hash citizen form.NewPassword
            do! Data.save
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
        return! Views.account form (isHtmx ctx) (csrf ctx) |> render "Account Profile" next ctx
}

// GET: /citizen/so-long
let soLong : HttpHandler = requireUser >=> fun next ctx ->
    Views.deletionOptions (csrf ctx) |> render "Account Deletion Options" next ctx

// ~~~ LEGACY MIGRATION ~~~ //

// GET: /citizen/legacy/list
let listLegacy : HttpHandler = Auth.requireAdmin >=> fun next ctx -> task {
    let! users = Data.legacy ()
    return! Views.listLegacy users |> render "Migrate Legacy Account" next ctx
}

open Giraffe.EndpointRouting

/// All endpoints for this feature
let endpoints =
    subRoute "/citizen" [
        GET_HEAD [
            route  "/account"           account
            routef "/cancel-reset/%s"   cancelReset
            routef "/confirm/%s"        confirm
            route  "/dashboard"         dashboard
            routef "/deny/%s"           deny
            route  "/forgot-password"   forgotPassword
            route  "/log-off"           logOff
            route  "/log-on"            logOn
            route  "/register"          register
            routef "/reset-password/%s" resetPassword
            route  "/so-long"           soLong
            route  "/legacy/list"       listLegacy
        ]
        POST [
            route "/delete"          delete
            route "/forgot-password" doForgotPassword
            route "/log-on"          doLogOn
            route "/register"        doRegistration
            route "/reset-password"  doResetPassword
            route "/save-account"    saveAccount
        ]
    ]
