/// Modules to provide support functions for types
[<AutoOpen>]
module JobsJobsJobs.Domain.Modules

open Markdig
open System
open Types

/// Format a GUID as a Short GUID
let private toShortGuid guid =
  let convert (g : Guid) =
    Convert.ToBase64String (g.ToByteArray ())
    |> String.map (fun x -> match x with '/' -> '_' | '+' -> '-' | _ -> x)
  (convert guid).Substring (0, 22)

/// Turn a Short GUID back into a GUID
let private fromShortGuid x =
  let unBase64 = x |> String.map (fun x -> match x with '_' -> '/' | '-' -> '+' | _ -> x)
  (Convert.FromBase64String >> Guid) $"{unBase64}=="


/// Support functions for citizen IDs
module CitizenId =
  /// Create a new citizen ID
  let create () = (Guid.NewGuid >> CitizenId) ()
  /// A string representation of a citizen ID
  let toString = function (CitizenId it) -> toShortGuid it
  /// Parse a string into a citizen ID
  let ofString = fromShortGuid >> CitizenId


/// Support functions for citizens
module Citizen =
  /// Get the name of the citizen (the first of real name, display name, or handle that is filled in)
  let name x =
    [ x.realName; x.displayName; Some x.mastodonUser ]
    |> List.find Option.isSome
    |> Option.get


/// Support functions for continent IDs
module ContinentId =
  /// Create a new continent ID
  let create () = (Guid.NewGuid >> ContinentId) ()
  /// A string representation of a continent ID
  let toString = function (ContinentId it) -> toShortGuid it
  /// Parse a string into a continent ID
  let ofString = fromShortGuid >> ContinentId


/// Support functions for listing IDs
module ListingId =
  /// Create a new job listing ID
  let create () = (Guid.NewGuid >> ListingId) ()
  /// A string representation of a listing ID
  let toString = function (ListingId it) -> toShortGuid it
  /// Parse a string into a listing ID
  let ofString = fromShortGuid >> ListingId


/// Support functions for Markdown strings
module MarkdownString =
  /// The Markdown conversion pipeline (enables all advanced features)
  let private pipeline = MarkdownPipelineBuilder().UseAdvancedExtensions().Build ()
  /// Convert this Markdown string to HTML
  let toHtml = function (Text text) -> Markdown.ToHtml (text, pipeline)


/// Support functions for Profiles
module Profile =
  // An empty profile
  let empty =
    { id                = CitizenId Guid.Empty
      seekingEmployment = false
      isPublic          = false
      continentId       = ContinentId Guid.Empty
      region            = ""
      remoteWork        = false
      fullTime          = false
      biography         = Text ""
      lastUpdatedOn     = NodaTime.Instant.MinValue
      experience        = None
      skills            = []
      }

/// Support functions for skill IDs
module SkillId =
  /// Create a new skill ID
  let create () = (Guid.NewGuid >> SkillId) ()
  /// A string representation of a skill ID
  let toString = function (SkillId it) -> toShortGuid it
  /// Parse a string into a skill ID
  let ofString = fromShortGuid >> SkillId


/// Support functions for success report IDs
module SuccessId =
  /// Create a new success report ID
  let create () = (Guid.NewGuid >> SuccessId) ()
  /// A string representation of a success report ID
  let toString = function (SuccessId it) -> toShortGuid it
  /// Parse a string into a success report ID
  let ofString = fromShortGuid >> SuccessId
