
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

/** The data required to show a viewable profile */
export interface ProfileForView {
  /** The profile itself */
  profile : Profile
  /** The citizen to whom the profile belongs */
  citizen : Citizen
  /** The continent for the profile */
  continent : Continent
}

/** A count */
export interface Count {
  /** The count being returned */
  count : number
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
