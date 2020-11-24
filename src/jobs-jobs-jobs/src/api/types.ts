/**
 * Client-side Type Definitions for Jobs, Jobs, Jobs.
 * 
 * @author Daniel J. Summers <daniel@bitbadger.solutions>
 * @version 1
 */

/**
 * A continent (one of the 7).
 */
export interface Continent {
  /** The ID of the continent */
  id: string

  /** The name of the continent */
  name: string
}

/**
 * A user's employment profile.
 */
export interface Profile {
  /** The ID of the user to whom the profile applies */
  citizenId: string

  /** Whether this user is actively seeking employment */
  seekingEmployment: boolean

  /** Whether information from this profile should appear in the public anonymous list of available skills */
  isPublic: boolean

  /** The continent on which the user is seeking employment */
  continent: Continent

  /** The region within that continent where the user would prefer to work */
  region: string

  /** Whether the user is looking for remote work */
  remoteWork: boolean

  /** Whether the user is looking for full-time work */
  fullTime: boolean

  /** The user's professional biography */
  biography: string

  /** When this profile was last updated */
  lastUpdatedOn: number

  /** The user's experience */
  experience?: string
}
