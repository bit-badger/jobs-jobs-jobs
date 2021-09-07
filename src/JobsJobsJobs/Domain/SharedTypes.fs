/// Types intended to be shared between the API and the client application
module JobsJobsJobs.Domain.SharedTypes

open JobsJobsJobs.Domain.Types
open Microsoft.Extensions.Options
open NodaTime

// fsharplint:disable FieldNames

/// The data required to add or edit a job listing
type ListingForm = {
  /// The ID of the listing
  id          : string
  /// The listing title
  title       : string
  /// The ID of the continent on which this opportunity exists
  continentId : string
  /// The region in which this opportunity exists
  region      : string
  /// Whether this is a remote work opportunity
  remoteWork  : bool
  /// The text of the job listing
  text        : string
  /// The date by which this job listing is needed
  neededBy    : string option
  }


/// The data needed to display a listing
type ListingForView = {
  /// The listing itself
  listing   : Listing
  /// The continent for that listing
  continent : Continent
}


/// The form submitted to expire a listing
type ListingExpireForm = {
  /// Whether the job was filled from here
  fromHere     : bool
  /// The success story written by the user
  successStory : string option
}


/// The various ways job listings can be searched
[<CLIMutable>]
type ListingSearch = {
  /// Retrieve job listings for this continent
  continentId : string option
  /// Text for a search within a region
  region      : string option
  /// Whether to retrieve job listings for remote work
  remoteWork  : string
  /// Text for a search with the job listing description
  text        : string option
  }


/// A successful logon
type LogOnSuccess = {
  /// The JSON Web Token (JWT) to use for API access
  jwt       : string
  /// The ID of the logged-in citizen (as a string)
  citizenId : string
  /// The name of the logged-in citizen
  name      : string
  }


/// A count
type Count = {
  // The count being returned
  count : int64
  }


/// An instance of a Mastodon server which is configured to work with Jobs, Jobs, Jobs
type MastodonInstance () =
  /// The name of the instance
  member val Name     = "" with get, set
  /// The URL for this instance
  member val Url      = "" with get, set
  /// The abbreviation used in the URL to distinguish this instance's return codes
  member val Abbr     = "" with get, set
  /// The client ID (assigned by the Mastodon server)
  member val ClientId = "" with get, set
  /// The cryptographic secret (provided by the Mastodon server)
  member val Secret   = "" with get, set


/// The authorization options for Jobs, Jobs, Jobs
type AuthOptions () =
  /// The host for the return URL for Mastodoon verification
  member val ReturnHost   = "" with get, set
  /// The secret with which the server signs the JWTs for auth once we've verified with Mastodon
  member val ServerSecret = "" with get, set
  /// The instances configured for use
  member val Instances    = Array.empty<MastodonInstance> with get, set
  interface IOptions<AuthOptions> with
    override this.Value = this


/// The Mastodon instance data provided via the Jobs, Jobs, Jobs API
type Instance = {
  /// The name of the instance
  name     : string
  /// The URL for this instance
  url      : string
  /// The abbreviation used in the URL to distinguish this instance's return codes
  abbr     : string
  /// The client ID (assigned by the Mastodon server)
  clientId : string
  }


/// The fields required for a skill
type SkillForm = {
  /// The ID of this skill
  id          : string
  /// The description of the skill
  description : string
  /// Notes regarding the skill
  notes       : string option
  }

/// The data required to update a profile
[<CLIMutable; NoComparison; NoEquality>]
type ProfileForm = {
  /// Whether the citizen to whom this profile belongs is actively seeking employment
  isSeekingEmployment : bool
  /// Whether this profile should appear in the public search
  isPublic            : bool
  /// The user's real name
  realName            : string
  /// The ID of the continent on which the citizen is located
  continentId         : string
  /// The area within that continent where the citizen is located
  region              : string
  /// If the citizen is available for remote work
  remoteWork          : bool
  /// If the citizen is seeking full-time employment
  fullTime            : bool
  /// The user's professional biography
  biography           : string
  /// The user's past experience
  experience          : string option
  /// The skills for the user
  skills              : SkillForm list
  }

/// Support functions for the ProfileForm type
module ProfileForm =
  /// Create an instance of this form from the given profile
  let fromProfile (profile : Types.Profile) =
    { isSeekingEmployment = profile.seekingEmployment
      isPublic            = profile.isPublic
      realName            = ""
      continentId         = string profile.continentId
      region              = profile.region
      remoteWork          = profile.remoteWork
      fullTime            = profile.fullTime
      biography           = match profile.biography with Text bio -> bio
      experience          = profile.experience |> Option.map (fun x -> match x with Text exp -> exp)
      skills              = profile.skills
                            |> List.map (fun s ->
                                { id          = string s.id
                                  description = s.description
                                  notes       = s.notes
                                  })
      }


/// The various ways profiles can be searched
[<CLIMutable>]
type ProfileSearch = {
  /// Retrieve citizens from this continent
  continentId   : string option
  /// Text for a search within a citizen's skills
  skill         : string option
  /// Text for a search with a citizen's professional biography and experience fields
  bioExperience : string option
  /// Whether to retrieve citizens who do or do not want remote work
  remoteWork    : string
  }


/// A user matching the profile search
type ProfileSearchResult = {
  /// The ID of the citizen
  citizenId         : CitizenId
  /// The citizen's display name
  displayName       : string
  /// Whether this citizen is currently seeking employment
  seekingEmployment : bool
  /// Whether this citizen is looking for remote work
  remoteWork        : bool
  /// Whether this citizen is looking for full-time work
  fullTime          : bool
  /// When this profile was last updated
  lastUpdatedOn     : Instant
  }


/// The data required to show a viewable profile
type ProfileForView = {
  /// The profile itself
  profile : Profile
  /// The citizen to whom the profile belongs
  citizen : Citizen
  /// The continent for the profile
  continent : Continent
}


/// The parameters for a public job search
[<CLIMutable>]
type PublicSearch = {
  /// Retrieve citizens from this continent
  continentId : string option
  /// Retrieve citizens from this region
  region : string option
  /// Text for a search within a citizen's skills
  skill : string option
  /// Whether to retrieve citizens who do or do not want remote work
  remoteWork : string
  }

/// Support functions for pblic searches
module PublicSearch =
  /// Is the search empty?
  let isEmptySearch (srch : PublicSearch) =
    [ srch.continentId
      srch.skill
      match srch.remoteWork with "" -> Some srch.remoteWork | _ -> None
      ]
    |> List.exists Option.isSome


/// A public profile search result
type PublicSearchResult = {
  /// The name of the continent on which the citizen resides
  continent  : string
  /// The region in which the citizen resides
  region     : string
  /// Whether this citizen is seeking remote work
  remoteWork : bool
  /// The skills this citizen has identified
  skills     : string list
  }


/// The data required to provide a success story
type StoryForm = {
  /// The ID of this story
  id       : string
  /// Whether the employment was obtained from Jobs, Jobs, Jobs
  fromHere : bool
  /// The success story
  story    : string
  }


/// An entry in the list of success stories
type StoryEntry = {
  /// The ID of this success story
  id          : SuccessId
  /// The ID of the citizen who recorded this story
  citizenId   : CitizenId
  /// The name of the citizen who recorded this story
  citizenName : string
  /// When this story was recorded
  recordedOn  : Instant
  /// Whether this story involves an opportunity that arose due to Jobs, Jobs, Jobs
  fromHere    : bool
  /// Whether this report has a further story, or if it is simply a "found work" entry
  hasStory    : bool
  }
