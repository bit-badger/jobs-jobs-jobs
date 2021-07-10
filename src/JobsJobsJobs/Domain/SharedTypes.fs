/// Types intended to be shared between the API and the client application
module JobsJobsJobs.Domain.SharedTypes

open JobsJobsJobs.Domain.Types

// fsharplint:disable FieldNames

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
