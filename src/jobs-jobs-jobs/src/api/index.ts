import { ref } from 'vue'
import { Profile } from './types'

/**
 * Jobs, Jobs, Jobs API interface
 * 
 * @author Daniel J. Summers <daniel@bitbadger.solutions>
 * @version 1
 */

/** The base URL for the Jobs, Jobs, Jobs API */
const API_URL = `${location.protocol}//${location.host}/api`

/** Local storage key for the Jobs, Jobs, Jobs access token */
const JJJ_TOKEN = 'jjj-token'

/**
 * A holder for the JSON Web Token (JWT) returned from Jobs, Jobs, Jobs
 */
class JwtHolder {
  private jwt: string | null = null

  /**
   * Get the current token (refreshing from local storage if needed).
   */
  get token(): string | null {
    if (!this.jwt) this.jwt = localStorage.getItem(JJJ_TOKEN)
    return this.jwt
  }

  /**
   * Set the current token (both here and in local storage).
   * 
   * @param tokn The token to be set
   */
  set token(tokn: string | null) {
    if (tokn) localStorage.setItem(JJJ_TOKEN, tokn); else localStorage.removeItem(JJJ_TOKEN)
    this.jwt = tokn
  }

  get hasToken(): boolean {
    return this.token !== null
  }
}

/** The user's current JWT */
const jwt = new JwtHolder()

/**
 * Execute an HTTP request using the fetch API.
 * 
 * @param url The URL to which the request should be made
 * @param method The HTTP method for the request (defaults to GET)
 * @param payload The payload to send along with the request (defaults to none)
 * @returns The response (if the request is successful)
 * @throws An error (if the request is unsuccessful)
 */
export async function doRequest(url: string, method?: string, payload?: string) {
  const headers: [string, string][] = [ [ 'Content-Type', 'application/json' ] ]
  if (jwt.hasToken) headers.push([ 'Authorization', `Bearer ${jwt.token}`])
  const options: RequestInit = {
    method: method || 'GET',
    headers: headers
  }
  if (method === 'POST' && payload) options.body = payload
  const actualUrl = (options.method === 'GET' && payload) ? `url?${payload}` : url
  const resp = await fetch(actualUrl, options)
  if (resp.ok || resp.status === 404) return resp
  throw new Error(`Error executing API request: ${resp.status} ~ ${resp.statusText}`)
}

/**
 * Authorize with Jobs, Jobs, Jobs using a No Agenda Social token.
 * 
 * @param nasToken The token obtained from No Agenda Social
 * @returns True if it is successful
 */
export async function jjjAuthorize(nasToken: string): Promise<boolean> {
  const resp = await doRequest(`${API_URL}/citizen/log-on`, 'POST', JSON.stringify({ accessToken: nasToken }))
  const jjjToken = await resp.json()
  jwt.token = jjjToken.accessToken
  return true
}

/**
 * Retrieve the employment profile for the current user.
 * 
 * @returns The profile if it is found; undefined otherwise
 */
export async function userProfile(): Promise<Profile | undefined> {
  const resp = await doRequest(`${API_URL}/profile`)
  if (resp.status === 404) {
    return undefined
  }
  const profile = await resp.json()
  return profile as Profile
}
