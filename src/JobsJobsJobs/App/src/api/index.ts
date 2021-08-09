import { MarkedOptions } from 'marked'
import {
  Citizen,
  Continent,
  Count,
  LogOnSuccess,
  Profile,
  ProfileForm,
  ProfileForView,
  ProfileSearch,
  ProfileSearchResult,
  PublicSearch,
  PublicSearchResult,
  StoryEntry,
  StoryForm,
  Success
} from './types'

/**
 * Create a URL that will access the API
 * @param url The partial URL for the API
 * @returns A full URL for the API
 */
const apiUrl = (url : string) : string => `http://localhost:5000/api/${url}`

/**
 * Create request init parameters
 *
 * @param method The method by which the request should be executed
 * @param user The currently logged-on user
 * @returns RequestInit parameters
 */
// eslint-disable-next-line
const reqInit = (method : string, user : LogOnSuccess, body : any | undefined = undefined) : RequestInit => {
  const headers = new Headers()
  headers.append('Authorization', `Bearer ${user.jwt}`)
  if (body) {
    headers.append('Content-Type', 'application/json')
    return {
      headers,
      method,
      cache: 'no-cache',
      body: JSON.stringify(body)
    }
  }
  return {
    headers,
    method
  }
}

/**
 * Retrieve a result for an API call
 *
 * @param resp The response received from the API
 * @param action The action being performed (used in error messages)
 * @returns The expected result (if found), undefined (if not found), or an error string
 */
async function apiResult<T> (resp : Response, action : string) : Promise<T | undefined | string> {
  if (resp.status === 200) return await resp.json() as T
  if (resp.status === 404) return undefined
  return `Error ${action} - ${await resp.text()}`
}

/**
 * Send an update via the API
 *
 * @param resp The response received from the API
 * @param action The action being performed (used in error messages)
 * @returns True (if the response is a success) or an error string
 */
async function apiSend (resp : Response, action : string) : Promise<boolean | string> {
  if (resp.status === 200) return true
  return `Error ${action} - (${resp.status}) ${await resp.text()}`
}

/**
 * Run an API action that does not return a result
 *
 * @param resp The response received from the API call
 * @param action The action being performed (used in error messages)
 * @returns Undefined (if successful), or an error string
 */
const apiAction = async (resp : Response, action : string) : Promise<string | undefined> => {
  if (resp.status === 200) return undefined
  return `Error ${action} - ${await resp.text()}`
}

export default {

  /** API functions for citizens */
  citizen: {

    /**
     * Log a citizen on
     *
     * @param code The authorization code from No Agenda Social
     * @returns The user result, or an error
     */
    logOn: async (code : string) : Promise<LogOnSuccess | string> => {
      const resp = await fetch(apiUrl(`citizen/log-on/${code}`), { method: 'GET', mode: 'cors' })
      if (resp.status === 200) return await resp.json() as LogOnSuccess
      return `Error logging on - ${await resp.text()}`
    },

    /**
     * Retrieve a citizen by their ID
     *
     * @param id The citizen ID to be retrieved
     * @param user The currently logged-on user
     * @returns The citizen, or an error
     */
    retrieve: async (id : string, user : LogOnSuccess) : Promise<Citizen | string | undefined> =>
      apiResult<Citizen>(await fetch(apiUrl(`citizen/get/${id}`), reqInit('GET', user)), `retrieving citizen ${id}`),

    /**
     * Delete the current citizen's entire Jobs, Jobs, Jobs record
     *
     * @param user The currently logged-on user
     * @returns Undefined if successful, an error if not
     */
    delete: async (user : LogOnSuccess) : Promise<string | undefined> =>
      apiAction(await fetch(apiUrl('citizen'), reqInit('DELETE', user)), 'deleting citizen')
  },

  /** API functions for continents */
  continent: {

    /**
     * Get all continents
     *
     * @returns All continents, or an error
     */
    all: async () : Promise<Continent[] | string | undefined> =>
      apiResult<Continent[]>(await fetch(apiUrl('continent/all'), { method: 'GET' }), 'retrieving continents')
  },

  /** API functions for profiles */
  profile: {

    /**
     * Clear the "seeking employment" flag on the current citizen's profile
     *
     * @param user The currently logged-on user
     * @returns True if the action was successful, or an error string if not
     */
    markEmploymentFound: async (user : LogOnSuccess) : Promise<boolean | string> => {
      const result = await fetch(apiUrl('profile/employment-found'), reqInit('PATCH', user))
      if (result.ok) return true
      return `${result.status} - ${result.statusText} (${await result.text()})`
    },

    /**
     * Search for public profile data using the given parameters
     *
     * @param query The public profile search parameters
     * @returns The matching public profiles (if found), undefined (if API returns 404), or an error string
     */
    publicSearch: async (query : PublicSearch) : Promise<PublicSearchResult[] | string | undefined> => {
      const params = new URLSearchParams()
      if (query.continentId) params.append('continentId', query.continentId)
      if (query.region) params.append('region', query.region)
      if (query.skill) params.append('skill', query.skill)
      params.append('remoteWork', query.remoteWork)
      return apiResult<PublicSearchResult[]>(
        await fetch(apiUrl(`profile/public-search?${params.toString()}`), { method: 'GET' }),
        'searching public profile data')
    },

    /**
     * Retrieve a profile
     *
     * @param id The ID of the profile to retrieve (optional; if omitted, retrieve for the current citizen)
     * @param user The currently logged-on user
     * @returns The profile (if found), undefined (if not found), or an error string
     */
    retreive: async (id : string | undefined, user : LogOnSuccess) : Promise<Profile | undefined | string> => {
      const url = id ? `profile/get/${id}` : 'profile'
      const resp = await fetch(apiUrl(url), reqInit('GET', user))
      if (resp.status === 200) return await resp.json() as Profile
      if (resp.status !== 204) return `Error retrieving profile - ${await resp.text()}`
    },

    /**
     * Retrieve a profile for viewing
     *
     * @param id The ID of the profile to retrieve for viewing
     * @param user The currently logged-on user
     * @returns The profile (if found), undefined (if not found), or an error string
     */
    retreiveForView: async (id : string, user : LogOnSuccess) : Promise<ProfileForView | string | undefined> =>
      apiResult<ProfileForView>(await fetch(apiUrl(`profile/view/${id}`), reqInit('GET', user)), 'retrieving profile'),

    /**
     * Save a user's profile data
     *
     * @param data The profile data to be saved
     * @param user The currently logged-on user
     * @returns True if the save was successful, an error string if not
     */
    save: async (data : ProfileForm, user : LogOnSuccess) : Promise<boolean | string> =>
      apiSend(await fetch(apiUrl('profile/save'), reqInit('POST', user, data)), 'saving profile'),

    /**
     * Search for profiles using the given parameters
     *
     * @param query The profile search parameters
     * @param user The currently logged-on user
     * @returns The matching profiles (if found), undefined (if API returns 404), or an error string
     */
    search: async (query : ProfileSearch, user : LogOnSuccess) : Promise<ProfileSearchResult[] | string | undefined> => {
      const params = new URLSearchParams()
      if (query.continentId) params.append('continentId', query.continentId)
      if (query.skill) params.append('skill', query.skill)
      if (query.bioExperience) params.append('bioExperience', query.bioExperience)
      params.append('remoteWork', query.remoteWork)
      return apiResult<ProfileSearchResult[]>(await fetch(apiUrl(`profile/search?${params.toString()}`),
        reqInit('GET', user)), 'searching profiles')
    },

    /**
     * Count profiles in the system
     *
     * @param user The currently logged-on user
     * @returns A count of profiles within the entire system
     */
    count: async (user : LogOnSuccess) : Promise<number | string> => {
      const resp = await fetch(apiUrl('profile/count'), reqInit('GET', user))
      if (resp.status === 200) {
        const result = await resp.json() as Count
        return result.count
      }
      return `Error counting profiles - ${await resp.text()}`
    },

    /**
     * Delete the current user's employment profile
     *
     * @param user The currently logged-on user
     * @returns Undefined if successful, an error if not
     */
    delete: async (user : LogOnSuccess) : Promise<string | undefined> =>
      apiAction(await fetch(apiUrl('profile'), reqInit('DELETE', user)), 'deleting profile')
  },

  /** API functions for success stories */
  success: {

    /**
     * Retrieve all success stories
     *
     * @param user The currently logged-on user
     * @returns All success stories (if any exist), undefined (if none exist), or an error
     */
    list: async (user : LogOnSuccess) : Promise<StoryEntry[] | string | undefined> =>
      apiResult<StoryEntry[]>(await fetch(apiUrl('success/list'), reqInit('GET', user)), 'retrieving success stories'),

    /**
     * Retrieve a success story by its ID
     *
     * @param id The success story ID to be retrieved
     * @param user The currently logged-on user
     * @returns The success story, or an error
     */
    retrieve: async (id : string, user : LogOnSuccess) : Promise<Success | string | undefined> =>
      apiResult<Success>(await fetch(apiUrl(`success/${id}`), reqInit('GET', user)), `retrieving success story ${id}`),

    /**
     * Save a success story
     *
     * @param data The data to be saved
     * @param user The currently logged-on user
     * @returns True if successful, an error string if not
     */
    save: async (data : StoryForm, user : LogOnSuccess) : Promise<boolean | string> =>
      apiSend(await fetch(apiUrl('success/save'), reqInit('POST', user, data)), 'saving success story')
  }
}

/** The standard Jobs, Jobs, Jobs options for `marked` (GitHub-Flavo(u)red Markdown (GFM) with smart quotes) */
export const markedOptions : MarkedOptions = {
  gfm: true,
  smartypants: true
}

export * from './types'
