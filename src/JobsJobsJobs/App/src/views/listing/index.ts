import { format } from "date-fns"

/**
 * Format the needed by date for display
 *
 * @param neededBy The defined needed by date
 * @returns The date to display
 */
export function formatNeededBy (neededBy : string) : string {
  return format(Date.parse(`${neededBy}T00:00:00`), "PPP")
}
