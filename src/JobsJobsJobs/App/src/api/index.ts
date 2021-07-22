import { LogOnSuccess } from './types'

/**
 * Create a URL that will access the API
 * @param url The partial URL for the API
 * @returns A full URL for the API
 */
const apiUrl = (url : string) : string => `/api/${url}`

export default {

  /**
   * Log a citizen on
   * @param code The authorization code from No Agenda Social
   * @returns The user result, or an error
   */
  logOn: async (code : string) : Promise<LogOnSuccess | string> => {
    const resp = await fetch(apiUrl(`/citizen/log-on/${code}`), { method: 'GET' })
    if (resp.status === 200) return await resp.json() as LogOnSuccess
    console.error(await resp.text())
    return 'Error logging on'
  }
}

export * from './types'
