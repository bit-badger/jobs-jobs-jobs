module JobsJobsJobs.Api.Data

open JobsJobsJobs.Api.Domain
open Npgsql.FSharp
open System

/// The connection URI for the database
let connectUri = Uri config.dbUri


/// Connect to the database
let db () =
  (Sql.fromUri >> Sql.connect) connectUri


/// Return None if the error is that a single row was expected, but no rows matched
let private noneIfNotFound (it : Async<Result<'T, exn>>) = async {
  match! it with
  | Ok x ->
      return (Some >> Ok) x
  | Error err ->
      return match err.Message with msg when msg.Contains "at least one" -> Ok None | _ -> Error err
  }


/// Get the item count from a single-row result
let private itemCount (read: RowReader) = read.int64 "item_count"


/// Functions for manipulating citizens
//    (SHUT UP, SLAVE!)
module Citizens =

  /// Create a Citizen from a row of data
  let private fromReader (read: RowReader) =
    match (read.string >> CitizenId.tryParse) "id" with
    | Ok citizenId -> {
          id          = citizenId
          naUser      = read.string            "na_user"
          displayName = read.string            "display_name"
          profileUrl  = read.string            "profile_url"
          joinedOn    = (read.int64 >> Millis) "joined_on"
          lastSeenOn  = (read.int64 >> Millis) "last_seen_on"
          }
    | Error err -> failwith err
  
  /// Determine if we already know about this user from No Agenda Social
  let findIdByNaUser naUser =
    db ()
    |> Sql.query "SELECT id FROM citizen WHERE na_user = @na_user"
    |> Sql.parameters [ "@na_user", Sql.string naUser ]
    |> Sql.executeRowAsync (fun read -> 
        match (read.string >> CitizenId.tryParse) "id" with
        | Ok citizenId -> citizenId
        | Error err -> failwith err)
    |> noneIfNotFound
  
  /// Add a citizen
  let add citizen =
    db ()
    |> Sql.query
        """INSERT INTO citizen (
              na_user, display_name, profile_url, joined_on, last_seen_on, id
            ) VALUES (
              @na_user, @display_name, @profile_url, @joined_on, @last_seen_on, @id
            )"""
    |> Sql.parameters [
          "@na_user",      Sql.string                         citizen.naUser
          "@display_name", Sql.string                         citizen.displayName
          "@profile_url",  Sql.string                         citizen.profileUrl
          "@joined_on",    (Millis.toLong      >> Sql.int64)  citizen.joinedOn
          "@last_seen_on", (Millis.toLong      >> Sql.int64)  citizen.lastSeenOn
          "@id",           (CitizenId.toString >> Sql.string) citizen.id
        ]
    |> Sql.executeNonQueryAsync

  /// Update a citizen record when they log on
  let update citizenId displayName =
    db ()
    |> Sql.query
        """UPDATE citizen
              SET display_name = @display_name,
                  last_seen_on = @last_seen_on
            WHERE id = @id"""
    |> Sql.parameters [
          "@display_name", Sql.string displayName
          "@last_seen_on", (DateTime.Now.toMillis >> Millis.toLong >> Sql.int64) ()
          "@id",           (CitizenId.toString >> Sql.string) citizenId
        ]
    |> Sql.executeNonQueryAsync

  /// Try to find a citizen with the given ID
  let tryFind citizenId =
    db ()
    |> Sql.query "SELECT * FROM citizen WHERE id = @id"
    |> Sql.parameters [ "@id", (CitizenId.toString >> Sql.string) citizenId ]
    |> Sql.executeRowAsync fromReader
    |> noneIfNotFound
