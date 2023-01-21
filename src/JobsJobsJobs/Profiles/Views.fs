/// Views for /profile URLs
module JobsJobsJobs.Profiles.Views

open Giraffe.Htmx.Common
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open JobsJobsJobs.Common.Views
open JobsJobsJobs.Domain
open JobsJobsJobs.Profiles.Domain

// ~~~ PROFILE EDIT ~~~ //

/// The profile edit menu page
let edit (profile : Profile) =
    let hasProfile = profile.Region <> ""
    pageWithTitle "Employment Profile" [
        p [] [ txt "There are three different sections to the employment profile." ]
        ul [] [
            li [ _class "mb-2" ] [
                a [ _href $"/profile/edit/general" ] [ strong [] [ txt "General Information" ] ]; br []
                txt "contains your location, professional biography, and information about the type of employment you "
                txt "may be seeking."
                if not hasProfile then txt " Entering information here will create your profile."
            ]
            if hasProfile then
                li [ _class "mb-2" ] [
                    let skillCount = List.length profile.Skills
                    a [ _href $"/profile/edit/skills" ] [ strong [] [ txt "Skills" ] ]; br []
                    txt "is where you can list skills you have acquired through education or experience."
                    em [] [
                        txt $" (Your profile currently lists {skillCount} skill"; if skillCount <> 1 then txt "s"
                        txt ".)"
                    ]
                ]
                li [ _class "mb-2" ] [
                    let historyCount = List.length profile.History
                    a [ _href $"/profile/edit/history" ] [ strong [] [ txt "Employment History" ] ]; br []
                    txt "is where you can record a chronological history of your employment."
                    em [] [
                        txt $" (Your profile contains {historyCount} employment history entr"
                        txt (if historyCount <> 1 then "ies" else "y"); txt ".)"
                    ]
                ]
        ]
        if hasProfile then
            p [] [
                a [ _class "btn btn-primary"; _href $"/profile/{CitizenId.toString profile.Id}/view" ] [
                    i [ _class "mdi mdi-file-account-outline" ] []; txt "&nbsp; View Your User Profile"
                ]
            ]
            hr []
            p [ _class "text-muted" ] [
                txt "If you want to delete your profile, or your entire account, "
                a [ _href "/citizen/so-long" ] [ txt "see your deletion options here" ]; txt "."
            ]
    ]


/// A link to allow the user to return to the profile edit menu page
let backToEdit =
    p [ _class "mx-3" ] [ a [ _href "/profile/edit" ] [ txt "&laquo; Back to Profile Edit Menu" ] ]


/// The profile edit page
let editGeneralInfo (m : EditProfileForm) continents csrf =
    pageWithTitle "Employment Profile: General Information" [
        backToEdit
        form [ _class "row g-3"; _action "/profile/save"; _hxPost "/profile/save" ] [
            antiForgery csrf
            div [ _class "col-12 col-sm-6 col-md-4" ] [
                continentList [] (nameof m.ContinentId) continents None m.ContinentId true
            ]
            div [ _class "col-12 col-sm-6 col-md-8" ] [
                textBox [ _type "text"; _maxlength "255" ] (nameof m.Region) m.Region "Region" true
                div [ _class "form-text" ] [ txt "Country, state, geographic area, etc." ]
            ]
            div [ _class "col-12" ] [
                checkBox [] (nameof m.IsSeekingEmployment) m.IsSeekingEmployment "I am currently seeking employment"
                if m.IsSeekingEmployment then
                    p [ _class "fst-italic " ] [
                        txt "If you have found employment, consider "
                        a [ _href "/success-story/new/edit" ] [ txt "telling your fellow citizens about it!" ]
                    ]
            ]
            div [ _class "col-12 col-offset-md-2 col-md-4" ] [
                checkBox [] (nameof m.RemoteWork) m.RemoteWork "I am interested in remote work"
            ]
            div [ _class "col-12 col-md-4" ] [
                checkBox [] (nameof m.FullTime) m.FullTime "I am interested in full-time work"
            ]
            markdownEditor [ _required ] (nameof m.Biography) m.Biography "Professional Biography"
            div [ _class "col-12" ] [
                hr []
                h4 [] [ txt "Experience" ]
                p [] [
                    txt "The information in this box is displayed after the list of skills and chronological job "
                    txt "history, with no heading; it can be used to highlight your experiences apart from the history "
                    txt "entries, provide closing notes, etc."
                ]
            ]
            markdownEditor [] (nameof m.Experience) (defaultArg m.Experience "") "Experience"
            div [ _class "col-12" ] [
                hr []
                h4 [] [ txt "Visibility" ]
                div [ _class "form-check" ] [
                    let pvt = ProfileVisibility.toString Private
                    input [ _type "radio"; _id $"{nameof m.Visibility}Private"; _name (nameof m.Visibility)
                            _class "form-check-input"; _value pvt; if m.Visibility = pvt then _checked ]
                    label [ _class "form-check-label"; _for $"{nameof m.Visibility}Private" ] [
                        strong [] [ txt "Private" ]
                        txt " &ndash; only show my employment profile to other authenticated users"
                    ]
                ]
                div [ _class "form-check" ] [
                    let anon = ProfileVisibility.toString Anonymous
                    input [ _type "radio"; _id $"{nameof m.Visibility}Anonymous"; _name (nameof m.Visibility)
                            _class "form-check-input"; _value anon; if m.Visibility = anon then _checked ]
                    label [ _class "form-check-label"; _for $"{nameof m.Visibility}Anonymous" ] [
                        strong [] [ txt "Anonymous" ]
                        txt " &ndash; show my location and skills to public users anonymously"
                    ]
                ]
                div [ _class "form-check" ] [
                    let pub = ProfileVisibility.toString Public
                    input [ _type "radio"; _id $"{nameof m.Visibility}Public"; _name (nameof m.Visibility)
                            _class "form-check-input"; _value pub; if m.Visibility = pub then _checked ]
                    label [ _class "form-check-label"; _for $"{nameof m.Visibility}Public" ] [
                        strong [] [ txt "Public" ]; txt " &ndash; show my full profile to public users"
                    ]
                ]
            ]
            div [ _class "col-12" ] [ submitButton "content-save-outline" "Save" ]
        ]
    ]


/// Render the skill edit template and existing skills
let skillForm (m : SkillForm) isNew =
    [   h4 [] [ txt $"""{if isNew then "Add a" else "Edit"} Skill""" ]
        div [ _class "col-12 col-md-6" ] [
            div [ _class "form-floating" ] [
                textBox [ _type "text"; _maxlength "200"; _autofocus ] (nameof m.Description) m.Description "Skill" true
            ]
            div [ _class "form-text" ] [ txt "A skill (language, design technique, process, etc.)" ]
        ]
        div [ _class "col-12 col-md-6" ] [
            div [ _class "form-floating" ] [
                textBox [ _type "text"; _maxlength "1000" ] (nameof m.Notes) m.Notes "Notes" false
            ]
            div [ _class "form-text" ] [ txt "A further description of the skill" ]
        ]
        div [ _class "col-12" ] [
            submitButton "content-save-outline" "Save"; txt " &nbsp; &nbsp; "
            a [ _href "/profile/edit/skills/list"; _hxGet "/profile/edit/skills/list"; _hxTarget "#skillList"
                _class "btn btn-secondary" ] [ i [ _class "mdi mdi-cancel"] []; txt "&nbsp; Cancel" ]
        ]
    ]


/// List the skills for an employment profile
let skillTable (skills : Skill list) editIdx csrf =
    let editingIdx = defaultArg editIdx -2
    let isEditing  = editingIdx >= -1
    let renderTable () =
        let editSkillForm skill idx =
            tr [] [
                td [ _colspan "3" ] [
                    form [ _class "row g-3"; _hxPost $"/profile/edit/skill/{idx}"; _hxTarget "#skillList" ] [
                        antiForgery csrf
                        yield! skillForm (SkillForm.fromSkill skill) (idx = -1)
                    ]
                ]
            ]
        table [ _class "table table-sm table-hover pt-3" ] [
            thead [] [
                [ "Action"; "Skill"; "Notes" ]
                |> List.map (fun it -> th [ _scope "col" ] [ txt it ])
                |> tr []
            ]
            tbody [] [
                if isEditing && editingIdx = -1 then editSkillForm { Skill.Description = ""; Notes = None } -1
                yield! skills |> List.mapi (fun idx skill ->
                    if isEditing && editingIdx = idx then editSkillForm skill idx
                    else
                        tr [] [
                            td [ if isEditing then _class "text-muted" ] [
                                if isEditing then txt "Edit ~ Delete"
                                else
                                    let link = $"/profile/edit/skill/{idx}"
                                    a [ _href link; _hxGet link ] [ txt "Edit" ]; txt " ~ "
                                    a [ _href $"{link}/delete"; _hxPost $"{link}/delete"; _class "text-danger"
                                        _hxConfirm "Are you sure you want to delete this skill?" ] [ txt "Delete" ]
                            ]
                            td [] [ str skill.Description ]
                            td [ if Option.isNone skill.Notes then _class "text-muted fst-italic" ] [
                                str (defaultArg skill.Notes "None")
                            ]
                        ])
            ]
        ]

    if List.isEmpty skills && not isEditing then
        p [ _id "skillList"; _class "text-muted fst-italic pt-3" ] [ txt "Your profile has no skills defined" ]
    else if List.isEmpty skills then
        form [ _id "skillList"; _hxTarget "this"; _hxPost "/profile/edit/skill/-1"; _hxSwap HxSwap.OuterHtml
               _class "row g-3" ] [
            antiForgery csrf
            yield! skillForm { Description = ""; Notes = "" } true
        ]
    else if isEditing then div [ _id "skillList" ] [ renderTable () ]
    else // not editing, there are skills to show
        form [ _id "skillList"; _hxTarget "this"; _hxSwap HxSwap.OuterHtml ] [
            antiForgery csrf
            renderTable ()
        ]


/// The profile skills maintenance page
let skills (skills : Skill list) csrf =
    pageWithTitle "Employment Profile: Skills" [
        backToEdit
        p [] [
            a [ _href "/profile/edit/skill/-1"; _hxGet "/profile/edit/skill/-1"; _hxTarget "#skillList"
                _hxSwap HxSwap.OuterHtml; _class "btn btn-sm btn-outline-primary rounded-pill" ] [ txt "Add a Skill" ]
        ]
        skillTable skills None csrf
    ]


/// The skill edit component
let editSkill (skills : Skill list) idx csrf =
    skillTable skills (Some idx) csrf


// ~~~ PROFILE SEARCH ~~~ //

/// The public search page
let publicSearch (m : PublicSearchForm) continents (results : PublicSearchResult list option) =
    pageWithTitle "People Seeking Work" [
        if Option.isNone results then
            p [] [
                txt "Enter one or more criteria to filter results, or just click &ldquo;Search&rdquo; to list all "
                txt "publicly searchable profiles."
            ]
        collapsePanel "Search Criteria" [
            form [ _class "container"; _method "GET"; _action "/profile/seeking" ] [
                input [ _type "hidden"; _name "searched"; _value "true" ]
                div [ _class "row" ] [
                    div [ _class "col-12 col-sm-6 col-md-4 col-lg-3" ] [
                        continentList [] "ContinentId" continents (Some "Any") m.ContinentId false
                    ]
                    div [ _class "col-12 col-sm-6 col-md-4 col-lg-3" ] [
                        textBox [ _maxlength "1000" ] (nameof m.Region) m.Region "Region" false
                        div [ _class "form-text" ] [ txt "(free-form text)" ]
                    ]
                    div [ _class "col-12 col-sm-6 col-offset-md-2 col-lg-3 col-offset-lg-0" ] [
                        label [ _class "jjj-label" ] [ txt "Seeking Remote Work?" ]; br []
                        div [ _class "form-check form-check-inline" ] [
                            input [ _type "radio"; _id "remoteNull"; _name (nameof m.RemoteWork); _value ""
                                    _class "form-check-input"; if m.RemoteWork = "" then _checked ]
                            label [ _class "form-check-label"; _for "remoteNull" ] [ txt "No Selection" ]
                        ]
                        div [ _class "form-check form-check-inline" ] [
                            input [ _type "radio"; _id "remoteYes"; _name (nameof m.RemoteWork); _value "yes"
                                    _class "form-check-input"; if m.RemoteWork = "yes" then _checked ]
                            label [ _class "form-check-label"; _for "remoteYes" ] [ txt "Yes" ]
                        ]
                        div [ _class "form-check form-check-inline" ] [
                            input [ _type "radio"; _id "remoteNo"; _name (nameof m.RemoteWork); _value "no"
                                    _class "form-check-input"; if m.RemoteWork = "no" then _checked ]
                            label [ _class "form-check-label"; _for "remoteNo" ] [ txt "No" ]
                        ]
                    ]
                    div [ _class "col-12 col-sm-6 col-lg-3" ] [
                        textBox [ _maxlength "1000" ] (nameof m.Skill) m.Skill "Skill" false
                        div [ _class "form-text" ] [ txt "(free-form text)" ]
                    ]
                ]
                div [ _class "row" ] [
                    div [ _class "col" ] [
                        br []
                        button [ _type "submit"; _class "btn btn-outline-primary" ] [ txt "Search" ]
                    ]
                ]
            ]
        ]
        match results with
        | Some r when List.isEmpty r -> p [ _class "pt-3" ] [ txt "No results found for the specified criteria" ]
        | Some r ->
            p [ _class "py-3" ] [
                txt "These profiles match your search criteria. To learn more about these people, join the merry band "
                txt "of human resources in the "
                a [ _href "https://noagendashow.net"; _target "_blank"; _rel "noopener" ] [ txt "No Agenda" ]
                txt " tribe!"
            ]
            table [ _class "table table-sm table-hover" ] [
                thead [] [
                    tr [] [
                        th [ _scope "col" ] [ txt "Continent" ]
                        th [ _scope "col"; _class "text-center" ] [ txt "Region" ]
                        th [ _scope "col"; _class "text-center" ] [ txt "Remote?" ]
                        th [ _scope "col"; _class "text-center" ] [ txt "Skills" ]
                    ]
                ]
                r |> List.map (fun profile ->
                    tr [] [
                        td [] [ str profile.Continent ]
                        td [] [ str profile.Region ]
                        td [ _class "text-center" ] [ txt (yesOrNo profile.RemoteWork) ]
                        profile.Skills
                        |> List.collect (fun skill -> [ str skill; br [] ])
                        |> td []
                    ])
                |> tbody []
            ]
        | None -> ()
    ]


/// Logged-on search page
let search (m : ProfileSearchForm) continents tz (results : ProfileSearchResult list option) =
    pageWithTitle "Search Profiles" [
        if Option.isNone results then
            p [] [
                txt "Enter one or more criteria to filter results, or just click &ldquo;Search&rdquo; to list all "
                txt "profiles."
            ]
        collapsePanel "Search Criteria" [
            form [ _class "container"; _method "GET"; _action "/profile/search" ] [
                input [ _type "hidden"; _name "searched"; _value "true" ]
                div [ _class "row" ] [
                    div [ _class "col-12 col-sm-6 col-md-4 col-lg-3" ] [
                        continentList [] "ContinentId" continents (Some "Any") m.ContinentId false
                    ]
                    div [ _class "col-12 col-sm-6 col-offset-md-2 col-lg-3 col-offset-lg-0" ] [
                        label [ _class "jjj-label" ] [ txt "Seeking Remote Work?" ]; br []
                        div [ _class "form-check form-check-inline" ] [
                            input [ _type "radio"; _id "remoteNull"; _name (nameof m.RemoteWork); _value ""
                                    _class "form-check-input"; if m.RemoteWork = "" then _checked ]
                            label [ _class "form-check-label"; _for "remoteNull" ] [ txt "No Selection" ]
                        ]
                        div [ _class "form-check form-check-inline" ] [
                            input [ _type "radio"; _id "remoteYes"; _name (nameof m.RemoteWork); _value "yes"
                                    _class "form-check-input"; if m.RemoteWork = "yes" then _checked ]
                            label [ _class "form-check-label"; _for "remoteYes" ] [ txt "Yes" ]
                        ]
                        div [ _class "form-check form-check-inline" ] [
                            input [ _type "radio"; _id "remoteNo"; _name (nameof m.RemoteWork); _value "no"
                                    _class "form-check-input"; if m.RemoteWork = "no" then _checked ]
                            label [ _class "form-check-label"; _for "remoteNo" ] [ txt "No" ]
                        ]
                    ]
                    div [ _class "col-12 col-sm-6 col-lg-3" ] [
                        textBox [ _maxlength "1000" ] (nameof m.Skill) m.Skill "Skill" false
                        div [ _class "form-text" ] [ txt "(free-form text)" ]
                    ]
                    div [ _class "col-12 col-sm-6 col-lg-3" ] [
                        textBox [ _maxlength "1000" ] (nameof m.BioExperience) m.BioExperience "Bio / Experience" false
                        div [ _class "form-text" ] [ txt "(free-form text)" ]
                    ]
                ]
                div [ _class "row" ] [
                    div [ _class "col" ] [
                        br []
                        button [ _type "submit"; _class "btn btn-outline-primary" ] [ txt "Search" ]
                    ]
                ]
            ]
        ]
        match results with
        | Some r when List.isEmpty r -> p [ _class "pt-3" ] [ txt "No results found for the specified criteria" ]
        | Some r ->
            // Bootstrap utility classes to only show at medium or above
            let isWide = "d-none d-md-table-cell"
            table [ _class "table table-sm table-hover pt-3" ] [
                thead [] [
                    tr [] [
                        th [ _scope "col" ] [ txt "Profile" ]
                        th [ _scope "col" ] [ txt "Name" ]
                        th [ _scope "col"; _class $"{isWide} text-center" ] [ txt "Seeking?" ]
                        th [ _scope "col"; _class "text-center" ] [ txt "Remote?" ]
                        th [ _scope "col"; _class $"{isWide} text-center" ] [ txt "Full-Time?" ]
                        th [ _scope "col"; _class isWide ] [ txt "Last Updated" ]
                    ]
                ]
                r |> List.map (fun profile ->
                    tr [] [
                        td [] [ a [ _href $"/profile/{CitizenId.toString profile.CitizenId}/view" ] [ txt "View" ] ]
                        td [ if profile.SeekingEmployment then _class "fw-bold" ] [ str profile.DisplayName ]
                        td [ _class $"{isWide} text-center" ] [ txt (yesOrNo profile.SeekingEmployment) ]
                        td [ _class "text-center" ] [ txt (yesOrNo profile.RemoteWork) ]
                        td [ _class $"{isWide} text-center" ] [ txt (yesOrNo profile.FullTime) ]
                        td [ _class isWide ] [ str (fullDate profile.LastUpdatedOn tz) ]
                    ])
                |> tbody []
            ]
        | None -> ()
    ]


/// Profile view template
let view (it : ProfileForView) currentId =
    article [] [
        h2 [] [
            str (Citizen.name it.Citizen)
            if it.Profile.IsSeekingEmployment then
                span [ _class "jjj-heading-label" ] [
                    txt "&nbsp; &nbsp;"; span [ _class "badge bg-dark" ] [ txt "Currently Seeking Employment" ]
                ]
        ]
        h4 [] [ str $"{it.Continent.Name}, {it.Profile.Region}" ]
        contactInfo it.Citizen (Option.isNone currentId)
        |> div [ _class "pb-3" ]
        p [] [
            txt (if it.Profile.IsFullTime then "I" else "Not i"); txt "nterested in full-time employment &bull; "
            txt (if it.Profile.IsRemote then "I" else "Not i"); txt "nterested in remote opportunities"
        ]
        hr []
        div [] [ md2html it.Profile.Biography ]
        if not (List.isEmpty it.Profile.Skills) then
            hr []
            h4 [ _class "pb-3" ] [ txt "Skills" ]
            it.Profile.Skills
            |> List.map (fun skill ->
                li [] [
                    str skill.Description
                    match skill.Notes with Some notes -> txt " &nbsp;("; str notes; txt ")" | None -> ()
                ])
            |> ul []
        match it.Profile.Experience with
        | Some exp -> hr []; h4 [ _class "pb-3" ] [ txt "Experience / Employment History" ]; div [] [ md2html exp ]
        | None -> ()
        if Option.isSome currentId && currentId.Value = it.Citizen.Id then
            br []; br []
            a [ _href "/profile/edit"; _class "btn btn-primary" ] [
                i [ _class "mdi mdi-pencil" ] []; txt "&nbsp; Edit Your Profile"
            ]
    ]
