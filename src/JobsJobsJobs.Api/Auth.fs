module JobsJobsJobs.Api.Auth

open FSharp.Json
open JWT

/// A JWT (de)serializer utilizing FSharp.Json
type FSharpJsonSerializer () =
  interface IJsonSerializer with
    member __.Serialize (any : obj) =
      Json.serialize any
    member __.Deserialize<'T> json =
      Json.deserialize<'T> json


open System.Net.Http
open System.Net.Http.Headers
open Data
open JWT.Builder
open JobsJobsJobs.Api.Domain
open JWT.Algorithms
open System

/// Verify a user's credentials with No Agenda Social
let verifyWithMastodon accessToken = async {
  use client = new HttpClient ()
  use req    = new HttpRequestMessage (HttpMethod.Get, (sprintf "%saccounts/verify_credentials" config.auth.apiUrl))
  req.Headers.Authorization <- AuthenticationHeaderValue <| sprintf "Bearer %s" accessToken
  match! client.SendAsync req |> Async.AwaitTask with
  | res when res.IsSuccessStatusCode ->
      let! body = res.Content.ReadAsStringAsync ()
      return
        match Json.deserialize<ViewModels.Citizen.MastodonAccount> body with
        | profile when profile.username = profile.acct -> Ok profile
        | profile -> Error (sprintf "Profiles must be from noagendasocial.com; yours is %s" profile.acct)
  | res -> return Error (sprintf "Could not retrieve credentials: %d ~ %s" (int res.StatusCode) res.ReasonPhrase)
  }

/// Create a JWT for the given user
let createJwt citizenId = async {
  match! Citizens.tryFind citizenId with
  | Ok (Some citizen) ->
      return
        JwtBuilder()
          .WithAlgorithm(HMACSHA256Algorithm ())
          // TODO: generate separate secret for server
          .WithSecret(config.auth.secret)
          .WithSerializer(FSharpJsonSerializer ())
          .AddClaim("sub", CitizenId.toString citizen.id)
          .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1.).ToUnixTimeSeconds ())
          .AddClaim("nam", citizen.displayName)
          .Encode ()
        |> Ok
  | Ok None -> return Error (exn "Citizen record not found")
  | Error exn -> return Error exn
  }
