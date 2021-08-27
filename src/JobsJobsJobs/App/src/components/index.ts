import { parseJSON } from 'date-fns'
import { utcToZonedTime } from 'date-fns-tz'

/**
 * Parse a date from its JSON representation to a UTC-aligned date
 *
 * @param date The date string in JSON from JSON
 * @returns A UTC JavaScript date
 */
export function parseToUtc (date : string) : Date {
  return utcToZonedTime(parseJSON(date), Intl.DateTimeFormat().resolvedOptions().timeZone)
}
