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

/// Create a floating-label text input box
let textBox attrs name value fieldLabel isRequired =
    div [ _class "form-floating" ] [
        List.append attrs [
            _id name; _name name; _class "form-control"; _placeholder fieldLabel; _value value
            if isRequired then _required
        ] |> input
        label [ _class (if isRequired then "jjj-required" else "jjj-label"); _for name ] [ rawText fieldLabel ]
    ]

/// Create a checkbox that will post "true" if checked
let checkBox name isChecked checkLabel =
    div [ _class "form-check" ] [
        input [ _type "checkbox"; _id name; _name name; _class "form-check-input"; _value "true"
                if isChecked then _checked ]
        label [ _class "form-check-label"; _for name ] [ str checkLabel ]
    ]

/// Create a select list of continents
let continentList attrs name (continents : Continent list) emptyLabel selectedValue isRequired =
    div [ _class "form-floating" ] [
        select (List.append attrs [ _id name; _name name; _class "form-select"; if isRequired then _required ]) (
            option [ _value ""; if selectedValue = "" then _selected ] [
                rawText $"""&ndash; {defaultArg emptyLabel "Select"} &ndash;""" ]
            :: (continents
                |> List.map (fun c ->
                    let theId = ContinentId.toString c.Id
                    option [ _value theId; if theId = selectedValue then _selected ] [ str c.Name ])))
        label [ _class (if isRequired then "jjj-required" else "jjj-label"); _for name ] [ rawText "Continent" ]
    ]

/// Create a Markdown editor
let markdownEditor attrs name value editorLabel =
    div [ _class "col-12" ] [
        nav [ _class "nav nav-pills pb-1" ] [
            button [ _type "button"; _id $"{name}EditButton"; _class "btn btn-primary btn-sm rounded-pill" ] [
                rawText "Markdown"
            ]
            rawText " &nbsp; "
            button [ _type "button"; _id $"{name}PreviewButton"
                     _class "btn btn-outline-secondary btn-sm rounded-pill" ] [
                rawText "Preview"
            ]
        ]
        section [ _id $"{name}Preview"; _class "jjj-not-shown jjj-markdown-preview px-2 pt-2"
                  _ariaLabel "Rendered Markdown preview" ] []
        div [ _id $"{name}Edit"; _class "form-floating jjj-shown" ] [
            textarea (List.append attrs
                                  [ _id name; _name name; _class "form-control jjj-markdown-editor"; _rows "10" ]) [
                rawText value
            ]
            label [ _for name ] [ rawText editorLabel ]
        ]
        script [] [
            rawText """document.addEventListener("DOMContentLoaded", function () {"""
            rawText $" jjj.markdownOnLoad('{name}') "
            rawText "})"
        ]
    ]

/// Wrap content in a collapsing panel
let collapsePanel header content =
    div [ _class "card" ] [
        div [ _class "card-body" ] [
            h6 [ _class "card-title" ] [
                // TODO: toggle collapse
                //a [ _href "#"; _class "{ 'cp-c': collapsed, 'cp-o': !collapsed }"; @click.prevent="toggle">{{headerText}} ]
                rawText header
            ]
            yield! content
        ]
    ]

/// "Yes" or "No" based on a boolean value
let yesOrNo value =
    if value then "Yes" else "No"

open NodaTime
open NodaTime.Text

/// Generate a full date from an instant in the citizen's local time zone
let fullDate (value : Instant) tz =
    (ZonedDateTimePattern.CreateWithCurrentCulture ("MMMM d, yyyy", DateTimeZoneProviders.Tzdb))
        .Format(value.InZone(DateTimeZoneProviders.Tzdb[tz]))
