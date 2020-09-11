[<AutoOpen>]
module JobsJobsJobs.Api.Extensions

open System

// fsharplint:disable MemberNames

/// Extensions for the DateTime object
type DateTime with
  
  /// Constant for the ticks at the Unix epoch
  member __.UnixEpochTicks = (DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks

  /// Convert this DateTime to a JavaScript milliseconds-past-the-epoch value
  member this.toMillis () =
    (this.ToUniversalTime().Ticks - this.UnixEpochTicks) / 10000L |> Domain.Millis


open FSharp.Json
open Suave
open System.Text

/// Extensions for Suave's context
type HttpContext with

  /// Deserialize an object from a JSON request body
  member this.fromJsonBody<'T> () =
    try
      Encoding.UTF8.GetString this.request.rawForm |> Json.deserialize<'T> |> Ok
    with x ->
      Error x
