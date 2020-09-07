/**
 * Authentication and Authorization.
 * 
 * This contains authentication and authorization functions needed to permit secure access to the application.
 * 
 * @author Daniel J. Summers <daniel@bitbadger.solutions>
 * @version 1
 */
import { CLIENT_SECRET } from './config'

/** Client ID for Jobs, Jobs, Jobs */
const CLIENT_ID = '6Ook3LBff00dOhyBgbf4eXSqIpAroK72aioIdGaDqxs'

/** No Agenda Social's base URL */
const NAS_URL = 'https://noagendasocial.com/'

/** No Agenda Social's base API URL */
const API_URL = `${NAS_URL}api/v1/`

/** The base URL for Jobs, Jobs, Jobs */
const JJJ_URL = `${location.protocol}//${location.host}/`

/**
 * Authorize access to this application from No Agenda Social.
 * 
 * This is the first step in a 2-step log on process; this step will prompt the user to authorize Jobs, Jobs, Jobs to
 * get information from their No Agenda Social profile. Once that authorization has been granted, we receive an access
 * code which we can use to request a full token.
 */
export function authorize() {
  const params = new URLSearchParams([
      [ 'client_id',     CLIENT_ID ],
      [ 'scope',         'read' ],
      [ 'redirect_uri',  `${JJJ_URL}user/authorized` ],
      [ 'response_type', 'code' ]
    ]).toString()
  location.assign(`${NAS_URL}oauth/authorize?${params}`)
}

export async function logOn(authCode: string) {
  const options: RequestInit = {
    method: 'POST',
    body: JSON.stringify({
      client_id:     CLIENT_ID,
      client_secret: CLIENT_SECRET,
      redirect_uri:  `${JJJ_URL}user/authorized`,
      grant_type:    'authorization_code',
      code:          authCode,
      scope:         'read'
    }),
    headers: { 'Content-Type': 'application/json' }
  }
  const resp = await fetch(`${NAS_URL}oauth/token`, options)
  if (resp.ok) {
    const token = await resp.json()
    // TODO: submit token to server, let server obtain profile from NA Social
    console.info(`Success - response ${JSON.stringify(token)}`)
  } else {
    // TODO: notify the user
    const err = await resp.text()
    console.error(`Failure - ${err}`)
  }
}
