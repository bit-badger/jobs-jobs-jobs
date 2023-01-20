/// Views for /profile URLs
module JobsJobsJobs.Profiles.Views

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open JobsJobsJobs.Common.Views
open JobsJobsJobs.Domain
open JobsJobsJobs.Profiles.Domain

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
    ]


/// Render the skill edit template and existing skills
let skillEdit (skills : SkillForm array) =
    let mapToInputs (idx : int) (skill : SkillForm) =
        div [ _id $"skillRow{idx}"; _class "row pb-3" ] [
            div [ _class "col-2 col-md-1 align-self-center" ] [
                button [ _class "btn btn-sm btn-outline-danger rounded-pill"; _title "Delete"
                         _onclick $"jjj.profile.removeSkill(idx)" ] [ txt "&nbsp;&minus;&nbsp;" ]
            ]
            div [ _class "col-10 col-md-6" ] [
                div [ _class "form-floating" ] [
                    input [ _type "text"; _id $"skillDesc{idx}"; _name $"Skills[{idx}].Description"
                            _class "form-control"; _placeholder "A skill (language, design technique, process, etc.)"
                            _maxlength "200"; _value skill.Description; _required ]
                    label [ _class "jjj-required"; _for $"skillDesc{idx}" ] [ txt "Skill" ]
                ]
                if idx < 1 then div [ _class "form-text" ] [ txt "A skill (language, design technique, process, etc.)" ]
            ]
            div [ _class "col-12 col-md-5" ] [
                div [ _class "form-floating" ] [
                    input [ _type "text"; _id $"skillNotes{idx}"; _name $"Skills[{idx}].Notes"; _class "form-control"
                            _maxlength "1000"; _placeholder "A further description of the skill (1,000 characters max)"
                            _value skill.Notes ]
                    label [ _class "jjj-label"; _for $"skillNotes{idx}" ] [ txt "Notes" ]
                ]
                if idx < 1 then div [ _class "form-text" ] [ txt "A further description of the skill" ]
            ]
        ]
    template [ _id "newSkill" ] [ mapToInputs -1 { Description = ""; Notes = "" } ]
    :: (skills |> Array.mapi mapToInputs |> List.ofArray)

/// The profile edit page
let editGeneralInfo (m : EditProfileForm) continents isNew citizenId csrf =
    pageWithTitle "My Employment Profile" [
        form [ _class "row g-3"; _action "/profile/save"; _hxPost "/profile/save" ] [
            antiForgery csrf
            div [ _class "col-12" ] [
                checkBox [] (nameof m.IsSeekingEmployment) m.IsSeekingEmployment "I am currently seeking employment"
                if m.IsSeekingEmployment then
                    p [ _class "fst-italic " ] [
                        txt "If you have found employment, consider "
                        a [ _href "/success-story/new/edit" ] [ txt "telling your fellow citizens about it!" ]
                    ]
            ]
            div [ _class "col-12 col-sm-6 col-md-4" ] [
                continentList [] (nameof m.ContinentId) continents None m.ContinentId true
            ]
            div [ _class "col-12 col-sm-6 col-md-8" ] [
                textBox [ _type "text"; _maxlength "255" ] (nameof m.Region) m.Region "Region" true
                div [ _class "form-text" ] [ txt "Country, state, geographic area, etc." ]
            ]
            markdownEditor [ _required ] (nameof m.Biography) m.Biography "Professional Biography"
            div [ _class "col-12 col-offset-md-2 col-md-4" ] [
                checkBox [] (nameof m.RemoteWork) m.RemoteWork "I am looking for remote work"
            ]
            div [ _class "col-12 col-md-4" ] [
                checkBox [] (nameof m.FullTime) m.FullTime "I am looking for full-time work"
            ]
            div [ _class "col-12" ] [
                hr []
                h4 [ _class "pb-2" ] [
                    txt "Skills &nbsp; "
                    button [ _type "button"; _class "btn btn-sm btn-outline-primary rounded-pill"
                             _onclick "jjj.profile.addSkill()" ] [ txt "Add a Skill" ]
                ]
            ]
            yield! skillEdit m.Skills
            div [ _class "col-12" ] [
                hr []
                h4 [] [ txt "Experience" ]
                p [] [
                    txt "This application does not have a place to individually list your chronological job history; "
                    txt "however, you can use this area to list prior jobs, their dates, and anything else you want to "
                    txt "include that&rsquo;s not already a part of your Professional Biography above."
                ]
            ]
            markdownEditor [] (nameof m.Experience) (defaultArg m.Experience "") "Experience"
            div [ _class "col-12 col-xl-6" ] [
                checkBox [] (nameof m.IsPubliclySearchable) m.IsPubliclySearchable
                         "Allow my profile to be searched publicly"
            ]
            div [ _class "col-12 col-xl-6" ] [
                checkBox [] (nameof m.IsPubliclyLinkable) m.IsPubliclyLinkable
                         "Show my profile to anyone who has the direct link to it"
            ]
            div [ _class "col-12" ] [
                submitButton "content-save-outline" "Save"
                if not isNew then
                    txt "&nbsp; &nbsp; "
                    a [ _class "btn btn-outline-secondary"; _href $"/profile/{CitizenId.toString citizenId}/view" ] [
                        i [ _color "#6c757d"; _class "mdi mdi-file-account-outline" ] []
                        txt "&nbsp; View Your User Profile"
                    ]
            ]
        ]
        hr []
        p [ _class "text-muted fst-italic" ] [
            txt "(If you want to delete your profile, or your entire account, "
            a [ _href "/citizen/so-long" ] [ txt "see your deletion options here" ]; txt ".)"
        ]
        jsOnLoad $"jjj.profile.nextIndex = {m.Skills.Length}"
    ]


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
