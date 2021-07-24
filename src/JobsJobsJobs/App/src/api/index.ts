import { Count, LogOnSuccess, Profile } from './types'

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
const reqInit = (method : string, user : LogOnSuccess) : RequestInit => {
  const headers = new Headers()
  headers.append('Authorization', `Bearer ${user.jwt}`)
  return {
    headers,
    method
    // mode: 'cors'
  }
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
    }
  },

  /** API functions for profiles */
  profile: {

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
      return `Error counting profiles = ${await resp.text()}`
    }
  }
}

export * from './types'
