/// Views for /profile URLs
[<RequireQualifiedAccess>]
module JobsJobsJobs.Views.Profile

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open JobsJobsJobs.Domain
open JobsJobsJobs.Domain.SharedTypes
open JobsJobsJobs.ViewModels

/// Render the skill edit template and existing skills
let skillEdit (skills : SkillForm array) =
    let mapToInputs (idx : int) (skill : SkillForm) =
        div [ _id $"skillRow{idx}"; _class "row pb-3" ] [
            div [ _class "col-2 col-md-1 align-self-center" ] [
                button [ _class "btn btn-sm btn-outline-danger rounded-pill"; _title "Delete"
                         _onclick $"jjj.profile.removeSkill(idx)" ] [
                    rawText "&nbsp;&minus;&nbsp;"
                ]
            ]
            div [ _class "col-10 col-md-6" ] [
                div [ _class "form-floating" ] [
                    input [ _type "text"; _id $"skillDesc{idx}"; _name $"Skills[{idx}].Description"
                            _class "form-control"; _placeholder "A skill (language, design technique, process, etc.)"
                            _maxlength "200"; _value skill.Description; _required ]
                    label [ _class "jjj-required"; _for $"skillDesc{idx}" ] [ rawText "Skill" ]
                ]
                if idx < 1 then
                    div [ _class "form-text" ] [ rawText "A skill (language, design technique, process, etc.)" ]
            ]
            div [ _class "col-12 col-md-5" ] [
                div [ _class "form-floating" ] [
                    input [ _type "text"; _id $"skillNotes{idx}"; _name $"Skills[{idx}].Notes"; _class "form-control"
                            _maxlength "1000"; _placeholder "A further description of the skill (1,000 characters max)"
                            _value skill.Notes ]
                    label [ _class "jjj-label"; _for $"skillNotes{idx}" ] [ rawText "Notes" ]
                ]
                if idx < 1 then
                    div [ _class "form-text" ] [ rawText "A further description of the skill" ]
            ]
        ]
    template [ _id "newSkill" ] [ mapToInputs -1 { Description = ""; Notes = "" } ]
    :: (skills |> Array.mapi mapToInputs |> List.ofArray)

/// The profile edit page
let edit (m : EditProfileViewModel) continents isNew citizenId csrf =
    article [] [
        h3 [ _class "pb-3" ] [ rawText "My Employment Profile" ]
        form [ _class "row g-3"; _action "/profile/save"; _hxPost "/profile/save" ] [
            antiForgery csrf
            div [ _class "col-12" ] [
                checkBox [] (nameof m.IsSeekingEmployment) m.IsSeekingEmployment "I am currently seeking employment"
                if m.IsSeekingEmployment then
                    p [] [
                        em [] [
                            rawText "If you have found employment, consider "
                            a [ _href "/success-story/new/edit" ] [ rawText "telling your fellow citizens about it!" ]
                        ]
                    ]
            ]
            div [ _class "col-12 col-sm-6 col-md-4" ] [
                continentList [] (nameof m.ContinentId) continents None m.ContinentId true
            ]
            div [ _class "col-12 col-sm-6 col-md-8" ] [
                textBox [ _type "text"; _maxlength "255" ] (nameof m.Region) m.Region "Region" true
                div [ _class "form-text" ] [ rawText "Country, state, geographic area, etc." ]
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
                    rawText "Skills &nbsp; "
                    button [ _type "button"; _class "btn btn-sm btn-outline-primary rounded-pill"
                             _onclick "jjj.profile.addSkill()" ] [
                        rawText "Add a Skill"
                    ]
                ]
            ]
            yield! skillEdit m.Skills
            div [ _class "col-12" ] [
                hr []
                h4 [] [ rawText "Experience" ]
                p [] [
                    rawText "This application does not have a place to individually list your chronological job "
                    rawText "history; however, you can use this area to list prior jobs, their dates, and anything "
                    rawText "else you want to include that&rsquo;s not already a part of your Professional Biography "
                    rawText "above."
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
                button [ _type "submit"; _class "btn btn-primary" ] [
                    i [ _class "mdi mdi-content-save-outline" ] []; rawText "&nbsp; Save"
                ]
                if not isNew then
                    rawText "&nbsp; &nbsp; "
                    a [ _class "btn btn-outline-secondary"; _href $"/profile/{CitizenId.toString citizenId}/view" ] [
                        i [ _color "#6c757d"; _class "mdi mdi-file-account-outline" ] []
                        rawText "&nbsp; View Your User Profile"
                    ]
            ]
        ]
        hr []
        p [ _class "text-muted fst-italic" ] [
            rawText "(If you want to delete your profile, or your entire account, "
            a [ _href "/citizen/so-long" ] [ rawText "see your deletion options here" ]; rawText ".)"
        ]
        script [] [
            rawText """addEventListener("DOMContentLoaded", function () {"""
            rawText $" jjj.profile.nextIndex = {m.Skills.Length} "
            rawText "})"
        ]
    ]


/// The public search page
let publicSearch (m : PublicSearchForm) continents (results : PublicSearchResult list option) =
    article [] [
        h3 [ _class "pb-3" ] [ rawText "People Seeking Work" ]
        if Option.isNone results then
            p [] [
                rawText "Enter one or more criteria to filter results, or just click &ldquo;Search&rdquo; to list all "
                rawText "publicly searchable profiles."
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
                        div [ _class "form-text" ] [ rawText "(free-form text)" ]
                    ]
                    div [ _class "col-12 col-sm-6 col-offset-md-2 col-lg-3 col-offset-lg-0" ] [
                        label [ _class "jjj-label" ] [ rawText "Seeking Remote Work?" ]; br []
                        div [ _class "form-check form-check-inline" ] [
                            input [ _type "radio"; _id "remoteNull"; _name (nameof m.RemoteWork); _value ""
                                    _class "form-check-input"; if m.RemoteWork = "" then _checked ]
                            label [ _class "form-check-label"; _for "remoteNull" ] [ rawText "No Selection" ]
                        ]
                        div [ _class "form-check form-check-inline" ] [
                            input [ _type "radio"; _id "remoteYes"; _name (nameof m.RemoteWork); _value "yes"
                                    _class "form-check-input"; if m.RemoteWork = "yes" then _checked ]
                            label [ _class "form-check-label"; _for "remoteYes" ] [ rawText "Yes" ]
                        ]
                        div [ _class "form-check form-check-inline" ] [
                            input [ _type "radio"; _id "remoteNo"; _name (nameof m.RemoteWork); _value "no"
                                    _class "form-check-input"; if m.RemoteWork = "no" then _checked ]
                            label [ _class "form-check-label"; _for "remoteNo" ] [ rawText "No" ]
                        ]
                    ]
                    div [ _class "col-12 col-sm-6 col-lg-3" ] [
                        textBox [ _maxlength "1000" ] (nameof m.Skill) m.Skill "Skill" false
                        div [ _class "form-text" ] [ rawText "(free-form text)" ]
                    ]
                ]
                div [ _class "row" ] [
                    div [ _class "col" ] [
                        br []
                        button [ _type "submit"; _class "btn btn-outline-primary" ] [ rawText "Search" ]
                    ]
                ]
            ]
        ]
        match results with
        | Some r when List.isEmpty r -> p [ _class "pt-3" ] [ rawText "No results found for the specified criteria" ]
        | Some r ->
            p [ _class "py-3" ] [
                rawText "These profiles match your search criteria. To learn more about these people, join the merry "
                rawText "band of human resources in the "
                a [ _href "https://noagendashow.net"; _target "_blank"; _rel "noopener" ] [ rawText "No Agenda" ]
                rawText " tribe!"
            ]
            table [ _class "table table-sm table-hover" ] [
                thead [] [
                    tr [] [
                        th [ _scope "col" ] [ rawText "Continent" ]
                        th [ _scope "col"; _class "text-center" ] [ rawText "Region" ]
                        th [ _scope "col"; _class "text-center" ] [ rawText "Remote?" ]
                        th [ _scope "col"; _class "text-center" ] [ rawText "Skills" ]
                    ]
                ]
                r |> List.map (fun profile ->
                    tr [] [
                        td [] [ str profile.Continent ]
                        td [] [ str profile.Region ]
                        td [ _class "text-center" ] [ rawText (yesOrNo profile.RemoteWork) ]
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
    article [] [
        h3 [ _class "pb-3" ] [ rawText "Search Profiles" ]
        if Option.isNone results then
            p [] [
                rawText "Enter one or more criteria to filter results, or just click &ldquo;Search&rdquo; to list all "
                rawText "profiles."
            ]
        collapsePanel "Search Criteria" [
            form [ _class "container"; _method "GET"; _action "/profile/search" ] [
                input [ _type "hidden"; _name "searched"; _value "true" ]
                div [ _class "row" ] [
                    div [ _class "col-12 col-sm-6 col-md-4 col-lg-3" ] [
                        continentList [] "ContinentId" continents (Some "Any") m.ContinentId false
                    ]
                    div [ _class "col-12 col-sm-6 col-offset-md-2 col-lg-3 col-offset-lg-0" ] [
                        label [ _class "jjj-label" ] [ rawText "Seeking Remote Work?" ]; br []
                        div [ _class "form-check form-check-inline" ] [
                            input [ _type "radio"; _id "remoteNull"; _name (nameof m.RemoteWork); _value ""
                                    _class "form-check-input"; if m.RemoteWork = "" then _checked ]
                            label [ _class "form-check-label"; _for "remoteNull" ] [ rawText "No Selection" ]
                        ]
                        div [ _class "form-check form-check-inline" ] [
                            input [ _type "radio"; _id "remoteYes"; _name (nameof m.RemoteWork); _value "yes"
                                    _class "form-check-input"; if m.RemoteWork = "yes" then _checked ]
                            label [ _class "form-check-label"; _for "remoteYes" ] [ rawText "Yes" ]
                        ]
                        div [ _class "form-check form-check-inline" ] [
                            input [ _type "radio"; _id "remoteNo"; _name (nameof m.RemoteWork); _value "no"
                                    _class "form-check-input"; if m.RemoteWork = "no" then _checked ]
                            label [ _class "form-check-label"; _for "remoteNo" ] [ rawText "No" ]
                        ]
                    ]
                    div [ _class "col-12 col-sm-6 col-lg-3" ] [
                        textBox [ _maxlength "1000" ] (nameof m.Skill) m.Skill "Skill" false
                        div [ _class "form-text" ] [ rawText "(free-form text)" ]
                    ]
                    div [ _class "col-12 col-sm-6 col-lg-3" ] [
                        textBox [ _maxlength "1000" ] (nameof m.BioExperience) m.BioExperience "Bio / Experience" false
                        div [ _class "form-text" ] [ rawText "(free-form text)" ]
                    ]
                ]
                div [ _class "row" ] [
                    div [ _class "col" ] [
                        br []
                        button [ _type "submit"; _class "btn btn-outline-primary" ] [ rawText "Search" ]
                    ]
                ]
            ]
        ]
        match results with
        | Some r when List.isEmpty r -> p [ _class "pt-3" ] [ rawText "No results found for the specified criteria" ]
        | Some r ->
            // Bootstrap utility classes to only show at medium or above
            let isWide = "d-none d-md-table-cell"
            table [ _class "table table-sm table-hover pt-3" ] [
                thead [] [
                    tr [] [
                        th [ _scope "col" ] [ rawText "Profile" ]
                        th [ _scope "col" ] [ rawText "Name" ]
                        th [ _scope "col"; _class $"{isWide} text-center" ] [ rawText "Seeking?" ]
                        th [ _scope "col"; _class "text-center" ] [ rawText "Remote?" ]
                        th [ _scope "col"; _class $"{isWide} text-center" ] [ rawText "Full-Time?" ]
                        th [ _scope "col"; _class isWide ] [ rawText "Last Updated" ]
                    ]
                ]
                r |> List.map (fun profile ->
                    tr [] [
                        td [] [ a [ _href $"/profile/{CitizenId.toString profile.CitizenId}/view" ] [ rawText "View" ] ]
                        td [ if profile.SeekingEmployment then _class "fw-bold" ] [ str profile.DisplayName ]
                        td [ _class $"{isWide} text-center" ] [ rawText (yesOrNo profile.SeekingEmployment) ]
                        td [ _class "text-center" ] [ rawText (yesOrNo profile.RemoteWork) ]
                        td [ _class $"{isWide} text-center" ] [ rawText (yesOrNo profile.FullTime) ]
                        td [ _class isWide ] [ str (fullDate profile.LastUpdatedOn tz) ]
                    ])
                |> tbody []
            ]
        | None -> ()
    ]


/// Profile view template
let view (citizen : Citizen) (profile : Profile) (continentName : string) pageTitle currentId =
    article [] [
        h3 [ _class "pb-3" ] [ str pageTitle ]
        h2 [] [
            // TODO: link to preferred profile
            a [ _href "#"; _target "_blank"; _rel "noopener" ] [ str (Citizen.name citizen) ]
            if profile.IsSeekingEmployment then
                span [ _class "jjj-heading-label" ] [
                    rawText "&nbsp; &nbsp;"; span [ _class "badge bg-dark" ] [ rawText "Currently Seeking Employment" ]
                ]
        ]
        h4 [ _class "pb-3" ] [ str $"{continentName}, {profile.Region}" ]
        p [] [
            rawText (if profile.IsFullTime then "I" else "Not i"); rawText "nterested in full-time employment"
            rawText " &bull; "
            rawText (if profile.IsRemote then "I" else "Not i"); rawText "nterested in remote opportunities"
        ]
        hr []
        div [] [ md2html profile.Biography ]
        if not (List.isEmpty profile.Skills) then
            hr []
            h4 [ _class "pb-3" ] [ rawText "Skills" ]
            profile.Skills
            |> List.map (fun skill ->
                li [] [
                    str skill.Description
                    match skill.Notes with
                    | Some notes ->
                        rawText " &nbsp;("; str notes; rawText ")"
                    | None -> ()
                ])
            |> ul []
        match profile.Experience with
        | Some exp -> hr []; h4 [ _class "pb-3" ] [ rawText "Experience / Employment History" ]; div [] [ md2html exp ]
        | None -> ()
        if Option.isSome currentId && currentId.Value = citizen.Id then
            br []; br []
            a [ _href "/profile/edit"; _class "btn btn-primary" ] [
                i [ _class "mdi mdi-pencil" ] []; rawText "&nbsp; Edit Your Profile"
            ]
    ]
