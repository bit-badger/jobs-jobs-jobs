[<AutoOpen>]
module JobsJobsJobs.Views.Common

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Accessibility
open Microsoft.AspNetCore.Antiforgery
open JobsJobsJobs.Domain

/// Create an audio clip with the specified text node
let audioClip clip text =
    span [ _class "jjj-audio-clip"; _onclick "jjj.playFile(this)" ] [
        text; audio [ _id clip ] [ source [ _src $"/audio/{clip}.mp3" ] ]
    ]

/// Create an anti-forgery hidden input
let antiForgery (csrf : AntiforgeryTokenSet) =
    input [ _type "hidden"; _name csrf.FormFieldName; _value csrf.RequestToken ]

/// Create a select list of continents
let continentList attrs name (continents : Continent list) emptyLabel selectedValue =
    div [ _class "form-floating" ] [
        select (List.append attrs [ _id name; _name name; _class "form-select" ]) (
            option [ _value ""; if selectedValue = "" then _selected ] [
                rawText $"""&ndash; {defaultArg emptyLabel "Select"} &ndash;""" ]
            :: (continents
                |> List.map (fun c ->
                    let theId = ContinentId.toString c.Id
                    option [ _value theId; if theId = selectedValue then _selected ] [ str c.Name ])))
        label [ _class "jjj-required"; _for name ] [ rawText "Continent" ]
    ]

/// Create a Markdown editor
let markdownEditor attrs name value editorLabel =
    div [ _class "col-12" ] [
        nav [ _class "nav nav-pills pb-1" ] [
            button [ _type "button"; _id $"{name}EditButton"; _class "btn btn-primary btn-sm rounded-pill" ] [
                rawText "Markdown"
            ]
            rawText " &nbsp; "
            button [ _type "button"; _id $"{name}PreviewButton"; _class "btn btn-outline-secondary btn-sm rounded-pill"
                     _onclick $"jjj.showPreview('{name}')" ] [
                rawText "Preview"
            ]
        ]
        section [ _id $"{name}Preview"; _class "jjj-not-shown"; _ariaLabel "Rendered Markdown preview" ] []
        div [ _id $"{name}Edit"; _class "form-floating jjj-shown" ] [
            textarea (List.append attrs
                                  [ _id name; _name name; _class "form-control jjj-markdown-editor"; _rows "10" ]) [
                rawText value
            ]
            label [ _for name ] [ rawText editorLabel ]
        ]
    ]
