module JobsJobsJobs.Api.Handlers

open Data
open Domain
open FSharp.Json
open Suave
open Suave.Operators
open System

[<AutoOpen>]
module private Internal =
  
  open Suave.Writers

  /// Send a JSON response
  let json x =
    Successful.OK (Json.serialize x)
    >=> setMimeType "application/json; charset=utf-8"


/// Handler to return the Vue application
module Vue =
  
  /// The application index page
  let app = Files.file "wwwroot/index.html"


/// Handlers for error conditions
module Error =

  open Suave.Logging
  open Suave.Logging.Message

  /// Handle errors
  let error (ex : Exception) msg =
    fun ctx ->
      seq {
        string ctx.request.url
        match msg with "" -> () | _ -> " ~ "; msg
        "\n"; (ex.GetType().Name); ": "; ex.Message; "\n"
        ex.StackTrace
        }
      |> Seq.reduce (+)
      |> (eventX >> ctx.runtime.logger.error)
      ServerErrors.INTERNAL_ERROR (Json.serialize {| error = ex.Message |}) ctx

  /// Handle 404s from the API, sending known URL paths to the Vue app so that they can be handled there
  let notFound : WebPart =
    fun ctx ->
        [ "/user"; "/jobs" ]
        |> List.filter ctx.request.path.StartsWith
        |> List.length
        |> function
        | 0 -> RequestErrors.NOT_FOUND "err" ctx
        | _ -> Vue.app ctx
      

/// /api/citizen route handlers
module Citizen =

  open ViewModels.Citizen

  /// Either add the user, or update their display name and last seen date
  let establishCitizen result profile = async {
    match result with
    | Some citId ->
        match! Citizens.update citId profile.displayName with
        | Ok _ -> return Ok citId
        | Error exn -> return Error exn
    | None ->
        let  now   = DateTime.Now.toMillis ()
        let! citId = CitizenId.create ()
        match! Citizens.add
              { id          = citId
                naUser      = profile.username
                displayName = profile.displayName
                profileUrl  = profile.url
                joinedOn    = now
                lastSeenOn  = now
                } with
        | Ok _ -> return Ok citId
        | Error exn -> return Error exn
    }

  /// POST: /api/citizen/log-on
  let logOn : WebPart =
    fun ctx -> async {
      match ctx.fromJsonBody<LogOn> () with
      | Ok data ->
          match! Auth.verifyWithMastodon data.accessToken with
          | Ok profile ->
              match! Citizens.findIdByNaUser profile.username with
              | Ok idResult ->
                  match! establishCitizen idResult profile with
                  | Ok citizenId ->
                      match! Auth.createJwt citizenId with
                      | Ok jwt -> return! json {| accessToken = jwt |} ctx
                      | Error exn -> return! Error.error exn "Could not issue access token" ctx
                  | Error exn -> return! Error.error exn "Could not update Jobs, Jobs, Jobs database" ctx
              | Error exn -> return! Error.error exn "Token not received" ctx
          | Error msg ->
              // Error message regarding exclusivity to No Agenda Social members
              return Some ctx
      | Error exn -> return! Error.error exn "Token not received" ctx
      }
      

open Suave.Filters

/// The routes for Jobs, Jobs, Jobs
let webApp =
  choose
    [ GET >=> choose
        [ path "/" >=> Vue.app
          Files.browse "wwwroot/"
          ]
      // PUT >=> choose
      //   [ ]
      // PATCH >=> choose
      //   [ ]
      POST >=> choose
        [ path "/api/citizen/log-on" >=> Citizen.logOn
          ]
      Error.notFound
      ]
