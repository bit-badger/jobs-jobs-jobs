/// Route handlers for Giraffe endpoints
module JobsJobsJobs.Api.Handlers

open System.IO
open Giraffe
open JobsJobsJobs.Common.Handlers
open JobsJobsJobs.Domain

// POST: /api/markdown-preview
let markdownPreview : HttpHandler = requireUser >=> fun next ctx -> task {
    let _ = ctx.Request.Body.Seek(0L, SeekOrigin.Begin)
    use reader = new StreamReader (ctx.Request.Body)
    let! preview = reader.ReadToEndAsync ()
    return! htmlString (MarkdownString.toHtml (Text preview)) next ctx
}


open Giraffe.EndpointRouting

/// All API endpoints
let endpoints =
    subRoute "/api" [
        POST [ route "/markdown-preview" markdownPreview ]
    ]
