
/** A successful logon */
export interface LogOnSuccess {
  /** The JSON Web Token (JWT) to use for API access */
  jwt : string
  /** The ID of the logged-in citizen (as a string) */
  citizenId : string
  /** The name of the logged-in citizen */
  name : string
}
