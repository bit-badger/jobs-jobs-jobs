/// Common helper functions for views
module JobsJobsJobs.Common.Handlers

open Giraffe
open Giraffe.Htmx
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

[<AutoOpen>]
module HtmxHelpers =
    
    /// Is the request from htmx?
    let isHtmx (ctx : HttpContext) =
        ctx.Request.IsHtmx && not ctx.Request.IsHtmxRefresh


/// Handlers for error conditions
module Error =
  
    open System.Net

    /// Handler that will return a status code 404 and the text "Not Found"
    let notFound : HttpHandler = fun _ ctx ->
        let fac  = ctx.GetService<ILoggerFactory> ()
        let log  = fac.CreateLogger "Handler"
        let path = string ctx.Request.Path
        log.LogInformation "Returning 404"
        RequestErrors.NOT_FOUND $"The URL {path} was not recognized as a valid URL" earlyReturn ctx
    
    
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
open System.Security.Claims
open System.Text.Json
open System.Text.RegularExpressions
open JobsJobsJobs.Domain
open Microsoft.AspNetCore.Antiforgery
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open NodaTime

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

/// Get the ID of the currently logged in citizen
//  NOTE: if no one is logged in, this will raise an exception
let currentCitizenId ctx = (tryUser >> Option.get >> CitizenId.ofString) ctx

let antiForgerySvc (ctx : HttpContext) =
    ctx.RequestServices.GetRequiredService<IAntiforgery> ()

/// Obtain an anti-forgery token set
let csrf ctx =
    (antiForgerySvc ctx).GetAndStoreTokens ctx

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

open JobsJobsJobs.Common.Views

/// Create the render context for an HTML response
let private createContext (ctx : HttpContext) pageTitle content messages : Layout.PageRenderContext =
    {   IsLoggedOn = Option.isSome (tryUser ctx)
        CurrentUrl = ctx.Request.Path.Value
        PageTitle  = pageTitle
        Content    = content
        Messages   = messages
    }

/// Render a page-level view
let render pageTitle (_ : HttpFunc) (ctx : HttpContext) content = task {
    let! messages   = popMessages ctx
    let  renderCtx  = createContext ctx pageTitle content messages
    let  renderFunc = if isHtmx ctx then Layout.partial else Layout.full
    return! ctx.WriteHtmlViewAsync (renderFunc renderCtx)
}

/// Render a printable view (content with styles, but no layout)
let renderPrint pageTitle (_ : HttpFunc) (ctx : HttpContext) content =
    createContext ctx pageTitle content []
    |> Layout.print
    |> ctx.WriteHtmlViewAsync

/// Render a bare (component) view
let renderBare (_ : HttpFunc) (ctx : HttpContext) content =
    createContext ctx "" content []
    |> Layout.bare
    |> ctx.WriteHtmlViewAsync

/// Render as a composable HttpHandler
let renderHandler pageTitle content : HttpHandler = fun next ctx ->
    render pageTitle next ctx content

/// Validate the anti cross-site request forgery token in the current request
let validateCsrf : HttpHandler = fun next ctx -> task {
    match! (antiForgerySvc ctx).IsRequestValidAsync ctx with
    | true -> return! next ctx
    | false -> return! RequestErrors.BAD_REQUEST "CSRF token invalid" earlyReturn ctx
}

/// Require a user to be logged on for a route
let requireUser : HttpHandler = requiresAuthentication Error.notAuthorized

/// Regular expression to validate that a URL is a local URL
let isLocal = Regex """^/[^\/\\].*"""

/// Redirect to another page, saving the session before redirecting
let redirectToGet (url : string) next ctx = task {
    do! saveSession ctx
    let action =
        if Option.isSome (noneIfEmpty url) && (url = "/" || isLocal.IsMatch url) then
            if isHtmx ctx then withHxRedirect url else redirectTo false url
        else RequestErrors.BAD_REQUEST "Invalid redirect URL"
    return! action next ctx
}

/// Shorthand for Error.notFound for use in handler functions
let notFound ctx =
    Error.notFound earlyReturn ctx
