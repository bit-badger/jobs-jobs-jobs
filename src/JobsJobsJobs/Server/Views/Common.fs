[<AutoOpen>]
module JobsJobsJobs.Views.Common

open Giraffe.ViewEngine
open Microsoft.AspNetCore.Antiforgery

/// Create an audio clip with the specified text node
let audioClip clip text =
    span [ _class "jjj-audio-clip"; _onclick "jjj.playFile(this)" ] [
        text; audio [ _id clip ] [ source [ _src $"/audio/{clip}.mp3" ] ]
    ]

/// Create an anti-forgery hidden input
let antiForgery (csrf : AntiforgeryTokenSet) =
    input [ _type "hidden"; _name csrf.FormFieldName; _value csrf.RequestToken ]
