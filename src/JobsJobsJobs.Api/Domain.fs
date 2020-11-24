module JobsJobsJobs.Api.Domain

// fsharplint:disable RecordFieldNames MemberNames

/// A short ID (12 characters of a Nano ID)
type ShortId =
| ShortId of string

/// Functions to maniuplate short IDs
module ShortId =
  
  open Nanoid
  open System.Text.RegularExpressions

  /// Regular expression to validate a string's format as a short ID
  let validShortId = Regex ("^[a-z0-9_-]{12}", RegexOptions.Compiled ||| RegexOptions.IgnoreCase)

  /// Convert a short ID to its string representation
  let toString = function ShortId text -> text

  /// Create a new short ID
  let create () = async {
    let! text = Nanoid.GenerateAsync (size = 12)
    return ShortId text
    }
  
  /// Try to parse a string into a short ID
  let tryParse (text : string) =
    match text.Length with
    | 12 when validShortId.IsMatch text -> (ShortId >> Ok) text
    | 12 -> Error "ShortId must be 12 characters [a-z,0-9,-, or _]"
    | x -> Error $"ShortId must be 12 characters; %d{x} provided"


/// The ID for a citizen (user) record
type CitizenId =
| CitizenId of ShortId

/// Functions for manipulating citizen (user) IDs
module CitizenId =
  /// Convert a citizen ID to its string representation
  let toString = function CitizenId shortId -> ShortId.toString shortId
  
  /// Create a new citizen ID
  let create () = async {
    let! shortId = ShortId.create ()
    return CitizenId shortId
    }

  /// Try to parse a string into a CitizenId
  let tryParse text =
    match ShortId.tryParse text with
    | Ok shortId -> (CitizenId >> Ok) shortId
    | Error err -> Error err


/// The ID for a continent record
type ContinentId =
| ContinentId of ShortId

/// Functions for manipulating continent IDs
module ContinentId =
  /// Convert a continent ID to its string representation
  let toString = function ContinentId shortId -> ShortId.toString shortId
  
  /// Create a new continent ID
  let create () = async {
    let! shortId = ShortId.create ()
    return ContinentId shortId
    }

  /// Try to parse a string into a ContinentId
  let tryParse text =
    match ShortId.tryParse text with
    | Ok shortId -> (ContinentId >> Ok) shortId
    | Error err -> Error err


/// The ID for a skill record
type SkillId =
| SkillId of ShortId

/// Functions for manipulating skill IDs
module SkillId =
  /// Convert a skill ID to its string representation
  let toString = function SkillId shortId -> ShortId.toString shortId
  
  /// Create a new skill ID
  let create () = async {
    let! shortId = ShortId.create ()
    return SkillId shortId
    }

  /// Try to parse a string into a CitizenId
  let tryParse text =
    match ShortId.tryParse text with
    | Ok shortId -> (SkillId >> Ok) shortId
    | Error err -> Error err


/// The ID for a success report record
type SuccessId =
| SuccessId of ShortId

/// Functions for manipulating success report IDs
module SuccessId =
  /// Convert a success report ID to its string representation
  let toString = function SuccessId shortId -> ShortId.toString shortId
  
  /// Create a new success report ID
  let create () = async {
    let! shortId = ShortId.create ()
    return SuccessId shortId
    }

  /// Try to parse a string into a SuccessId
  let tryParse text =
    match ShortId.tryParse text with
    | Ok shortId -> (SuccessId >> Ok) shortId
    | Error err -> Error err


/// A number representing milliseconds since the epoch (AKA JavaScript time)
type Millis =
| Millis of int64

/// Functions to manipulate ticks
module Millis =
  /// Convert a Ticks instance to its primitive value
  let toLong = function Millis millis -> millis


/// A string that holds Markdown-formatted text
type MarkdownString =
| MarkdownString of string

/// Functions to manipulate Markdown-formatted text
module MarkdownString =
  
  open Markdig

  /// Markdown pipeline that supports all built-in Markdown extensions
  let private pipeline = MarkdownPipelineBuilder().UseAdvancedExtensions().Build ()

  /// Get the plain-text (non-rendered) representation of the text
  let toText = function MarkdownString str -> str

  /// Get the HTML (rendered) representation of the text
  let toHtml = function MarkdownString str -> Markdown.ToHtml (str, pipeline)



/// A user
type Citizen = {
  /// The ID of the user
  id          : CitizenId
  /// The user's handle on No Agenda Social
  naUser      : string
  /// The user's display name from No Agenda Social (as of their last login here)
  displayName : string
  /// The URL to the user's profile on No Agenda Social
  profileUrl  : string
  /// When the user signed up here
  joinedOn    : Millis
  /// When the user last logged on here
  lastSeenOn  : Millis
}


/// A continent
type Continent = {
  /// The ID of the continent
  id   : ContinentId
  /// The name of the continent
  name : string
}


/// An employment / skills profile
type Profile = {
  /// The ID of the user to whom the profile applies
  citizenId         : CitizenId
  /// Whether this user is actively seeking employment
  seekingEmployment : bool
  /// Whether information from this profile should appear in the public anonymous list of available skills
  isPublic          : bool
  /// The continent on which the user is seeking employment
  continent         : Continent
  /// The region within that continent where the user would prefer to work
  region            : string
  /// Whether the user is looking for remote work
  remoteWork        : bool
  /// Whether the user is looking for full-time work
  fullTime          : bool
  /// The user's professional biography
  biography         : MarkdownString
  /// When this profile was last updated
  lastUpdatedOn     : Millis
  /// The user's experience
  experience        : MarkdownString option
}


/// A skill which a user possesses
type Skill = {
  /// The ID of the skill
  id        : SkillId
  /// The ID of the user who possesses this skill
  citizenId : CitizenId
  /// The skill
  skill     : string
  /// Notes about the skill (proficiency, experience, etc.)
  notes     : string option
}


/// A success story
type Success = {
  /// The ID of the success story
  id         : SuccessId
  /// The ID of the user who experienced this success story
  citizenId  : CitizenId
  /// When this story was recorded
  recordedOn : Millis
  /// Whether the success came from here; if Jobs, Jobs, Jobs led them to eventual employment
  fromHere   : bool
  /// Their success story
  story      : MarkdownString option
}


/// Configuration required for authentication with No Agenda Social
type AuthConfig = {
  /// The client ID
  clientId   : string
  /// The cryptographic secret
  secret     : string
  /// The base URL for Mastodon's API access
  apiUrl     : string
  }

/// Application configuration format
type JobsJobsJobsConfig = {
  /// Auth0 configuration
  auth : AuthConfig
  /// Database connection URI
  dbUri : string
  }


open Microsoft.Extensions.Configuration
open System.IO

/// Configuration instance
let config =
  (lazy
    (let root =
      ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory ())
        .AddJsonFile("appsettings.json")
        .AddJsonFile("appsettings.Development.json", true)
        .AddJsonFile("appsettings.Production.json", true)
        .AddEnvironmentVariables("JJJ_")
        .Build()
     let auth = root.GetSection "Auth"
    { dbUri = root.["dbUri"]
      auth  = {
        clientId = auth.["ClientId"]
        secret   = auth.["Secret"]
        apiUrl   = auth.["ApiUrl"]
        }
      })).Force()
