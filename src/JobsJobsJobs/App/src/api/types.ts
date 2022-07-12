
/** A user of Jobs, Jobs, Jobs */
export interface Citizen {
  /** The ID of the user */
  id : string
  /** The abbreviation of the instance where this citizen is based */
  instance : string
  /** The handle by which the user is known on Mastodon */
  mastodonUser : string
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

/** The Mastodon instance data provided via the Jobs, Jobs, Jobs API */
export interface Instance {
  /** The name of the instance */
  name : string
  /** The URL for this instance */
  url : string
  /** The abbreviation used in the URL to distinguish this instance's return codes */
  abbr : string
  /** The client ID (assigned by the Mastodon server) */
  clientId : string
  /** Whether this instance is enabled */
  isEnabled : boolean
  /** If disabled, the reason why it is disabled */
  reason : string
}

/** A job listing */
export interface Listing {
  /** The ID of the job listing */
  id : string
  /** The ID of the citizen who posted the job listing */
  citizenId : string
  /** When this job listing was created (date) */
  createdOn : string
  /** The short title of the job listing */
  title : string
  /** The ID of the continent on which the job is located */
  continentId : string
  /** The region in which the job is located */
  region : string
  /** Whether this listing is for remote work */
  remoteWork : boolean
  /** Whether this listing has expired */
  isExpired : boolean
  /** When this listing was last updated (date) */
  updatedOn : string
  /** The details of this job */
  text : string
  /** When this job needs to be filled (date) */
  neededBy : string | undefined
  /** Was this job filled as part of its appearance on Jobs, Jobs, Jobs? */
  wasFilledHere : boolean | undefined
}

/** The data required to add or edit a job listing */
export class ListingForm {
  /** The ID of the listing */
  id = ""
  /** The listing title */
  title = ""
  /** The ID of the continent on which this opportunity exists */
  continentId = ""
  /** The region in which this opportunity exists */
  region = ""
  /** Whether this is a remote work opportunity */
  remoteWork = false
  /** The text of the job listing */
  text = ""
  /** The date by which this job listing is needed */
  neededBy : string | undefined
}

/** The form submitted to expire a listing */
export class ListingExpireForm {
  /** Whether the job was filled from here */
  fromHere = false
  /** The success story written by the user */
  successStory : string | undefined
}

/** The data required to view a listing */
export interface ListingForView {
  /** The listing itself */
  listing : Listing
  /** The continent for the listing */
  continent : Continent
}

/** The various ways job listings can be searched */
export interface ListingSearch {
  /** Retrieve opportunities from this continent */
  continentId : string | undefined
  /** Text for a search for a specific region */
  region : string | undefined
  /** Whether to retrieve job listings for remote work */
  remoteWork : string
  /** Text to search with a job's full description */
  text : string | undefined
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
export class ProfileForm {
  /** Whether the citizen to whom this profile belongs is actively seeking employment */
  isSeekingEmployment = false
  /** Whether this profile should appear in the public search */
  isPublic = false
  /** The user's real name */
  realName = ""
  /** The ID of the continent on which the citizen is located */
  continentId = ""
  /** The area within that continent where the citizen is located */
  region = ""
  /** If the citizen is available for remote work */
  remoteWork = false
  /** If the citizen is seeking full-time employment */
  fullTime = false
  /** The user's professional biography */
  biography = ""
  /** The user's past experience */
  experience : string | undefined
  /** The skills for the user */
  skills : Skill[] = []
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

/** The data required to provide a success story */
export class StoryForm {
  /** The ID of this story */
  id = ""
  /** Whether the employment was obtained from Jobs, Jobs, Jobs */
  fromHere = false
  /** The success story */
  story = ""
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
