
/** A user of Jobs, Jobs, Jobs */
export interface Citizen {
  /** The ID of the user */
  id : string
  /** The handle by which the user is known on Mastodon */
  naUser : string
  /** The user's display name from Mastodon (updated every login) */
  displayName : string | undefined
  /** The user's real name */
  realName : string | undefined
  /** The URL for the user's Mastodon profile */
  profileUrl : string
  /** When the user joined Jobs, Jobs, Jobs (date) */
  joinedOn : string
  /** When the user last logged in (date) */
  lastSeenOn : string
}

/** A continent */
export interface Continent {
  /** The ID of the continent */
  id : string
  /** The name of the continent */
  name : string
}

/** A count */
export interface Count {
  /** The count being returned */
  count : number
}

/** A successful logon */
export interface LogOnSuccess {
  /** The JSON Web Token (JWT) to use for API access */
  jwt : string
  /** The ID of the logged-in citizen (as a string) */
  citizenId : string
  /** The name of the logged-in citizen */
  name : string
}

/** A skill the job seeker possesses */
export interface Skill {
  /** The ID of the skill */
  id : string
  /** A description of the skill */
  description : string
  /** Notes regarding this skill (level, duration, etc.) */
  notes : string | undefined
}

/** A job seeker profile */
export interface Profile {
  /** The ID of the citizen to whom this profile belongs */
  id : string
  /** Whether this citizen is actively seeking employment */
  seekingEmployment : boolean
  /** Whether this citizen allows their profile to be a part of the publicly-viewable, anonymous data */
  isPublic : boolean
  /** The ID of the continent on which the citizen resides */
  continentId : string
  /** The region in which the citizen resides */
  region : string
  /** Whether the citizen is looking for remote work */
  remoteWork : boolean
  /** Whether the citizen is looking for full-time work */
  fullTime : boolean
  /** The citizen's professional biography */
  biography : string
  /** When the citizen last updated their profile (date) */
  lastUpdatedOn : string
  /** The citizen's experience (topical / chronological) */
  experience : string | undefined
  /** Skills this citizen possesses */
  skills : Skill[]
}

/** The data required to update a profile */
export interface ProfileForm {
  /** Whether the citizen to whom this profile belongs is actively seeking employment */
  isSeekingEmployment : boolean
  /** Whether this profile should appear in the public search */
  isPublic : boolean
  /** The user's real name */
  realName : string
  /** The ID of the continent on which the citizen is located */
  continentId : string
  /** The area within that continent where the citizen is located */
  region : string
  /** If the citizen is available for remote work */
  remoteWork : boolean
  /** If the citizen is seeking full-time employment */
  fullTime : boolean
  /** The user's professional biography */
  biography : string
  /** The user's past experience */
  experience : string | undefined
  /** The skills for the user */
  skills : Skill[]
}

/** The data required to show a viewable profile */
export interface ProfileForView {
  /** The profile itself */
  profile : Profile
  /** The citizen to whom the profile belongs */
  citizen : Citizen
  /** The continent for the profile */
  continent : Continent
}

/** The various ways profiles can be searched */
export interface ProfileSearch {
  /** Retrieve citizens from this continent */
  continentId : string | undefined
  /** Text for a search within a citizen's skills */
  skill : string | undefined
  /** Text for a search with a citizen's professional biography and experience fields */
  bioExperience : string | undefined
  /** Whether to retrieve citizens who do or do not want remote work */
  remoteWork : string
}

/** A user matching the profile search */
export interface ProfileSearchResult {
  /** The ID of the citizen */
  citizenId : string
  /** The citizen's display name */
  displayName : string
  /** Whether this citizen is currently seeking employment */
  seekingEmployment : boolean
  /** Whether this citizen is looking for remote work */
  remoteWork : boolean
  /** Whether this citizen is looking for full-time work */
  fullTime : boolean
  /** When this profile was last updated (date) */
  lastUpdatedOn : string
}

/** The parameters for a public job search */
export interface PublicSearch {
  /** Retrieve citizens from this continent */
  continentId : string | undefined
  /** Retrieve citizens from this region */
  region : string | undefined
  /** Text for a search within a citizen's skills */
  skill : string | undefined
  /** Whether to retrieve citizens who do or do not want remote work */
  remoteWork : string
}

/** A public profile search result */
export interface PublicSearchResult {
  /** The name of the continent on which the citizen resides */
  continent : string
  /** The region in which the citizen resides */
  region : string
  /** Whether this citizen is seeking remote work */
  remoteWork : boolean
  /** The skills this citizen has identified */
  skills : string[]
}

/** An entry in the list of success stories */
export interface StoryEntry {
  /** The ID of this success story */
  id : string
  /** The ID of the citizen who recorded this story */
  citizenId : string
  /** The name of the citizen who recorded this story */
  citizenName : string
  /** When this story was recorded (date) */
  recordedOn : string
  /** Whether this story involves an opportunity that arose due to Jobs, Jobs, Jobs */
  fromHere : boolean
  /** Whether this report has a further story, or if it is simply a "found work" entry */
  hasStory : boolean
}

/** A record of success finding employment */
export interface Success {
  /** The ID of the success report */
  id : string
  /** The ID of the citizen who wrote this success report */
  citizenId : string
  /** When this success report was recorded (date) */
  recordedOn : string
  /** Whether the success was due, at least in part, to Jobs, Jobs, Jobs */
  fromHere : boolean
  /** The source of this success (listing or profile) */
  source : string
  /** The success story */
  story : string | undefined
}
