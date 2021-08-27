import { sanitize } from "dompurify"
import marked from "marked"

/**
 * Transform Markdown to HTML (standardize option, sanitize the output)
 *
 * @param markdown The Markdown text to be rendered as HTML
 * @returns The rendered HTML
 */
export function toHtml (markdown : string) : string {
  return sanitize(marked(markdown, { gfm: true, smartypants: true }), { USE_PROFILES: { html: true } })
}
