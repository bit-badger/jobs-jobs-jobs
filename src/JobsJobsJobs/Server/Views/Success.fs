/// Views for /success-stor[y|ies] URLs
[<RequireQualifiedAccess>]
module JobsJobsJobs.Views.Success

open Giraffe.ViewEngine
open JobsJobsJobs.Domain
open JobsJobsJobs.Domain.SharedTypes
open JobsJobsJobs.ViewModels

/// The add/edit success story page
let edit (m : EditSuccessForm) isNew pgTitle csrf =
    article [] [
        h3 [ _class "pb-3" ] [ rawText pgTitle ]
        if isNew then
            p [] [
                rawText "Congratulations on your employment! Your fellow citizens would enjoy hearing how it all came "
                rawText "about; tell us about it below! "
                em [] [ rawText "(These will be visible to other users, but not to the general public.)" ]
            ]
        form [ _class "row g-3"; _method "POST"; _action "/success-story/save" ] [
            antiForgery csrf
            input [ _type "hidden"; _name (nameof m.Id); _value m.Id ]
            div [ _class "col-12" ] [
                checkBox [] (nameof m.FromHere) m.FromHere "I found my employment here"
            ]
            markdownEditor [] (nameof m.Story) m.Story "The Success Story"
            div [ _class "col-12" ] [
                button [ _type "submit"; _class "btn btn-primary" ] [
                    i [ _class "mdi mdi-content-save-outline" ] []; rawText "&nbsp; Save"
                ]
                if isNew then
                    p [ _class "fst-italic" ] [
                        rawText "(Saving this will set &ldquo;Seeking Employment&rdquo; to &ldquo;No&rdquo; on your "
                        rawText "profile.)"
                    ]
            ]
        ]
    ]


/// The list of success stories
let list (m : StoryEntry list) citizenId tz =
    article [] [
        h3 [ _class "pb-3" ] [ rawText "Success Stories" ]
        if List.isEmpty m then
            p [] [ rawText "There are no success stories recorded "; em [] [ rawText "(yet)" ] ]
        else
            table [ _class "table table-sm table-hover" ] [
                thead [] [
                    [ "Story"; "From"; "Found Here?"; "Recorded On" ]
                    |> List.map (fun it -> th [ _scope "col" ] [ rawText it ])
                    |> tr []
                ]
                m |> List.map (fun story ->
                    tr [] [
                        td [] [
                            let theId = SuccessId.toString story.Id
                            if story.HasStory then a [ _href $"/success-story/{theId}/view" ] [ rawText "View" ]
                            else em [] [ rawText "None" ]
                            if story.CitizenId = citizenId then
                                rawText " ~ "; a [ _href $"/success-story/{theId}/edit" ] [ rawText "Edit" ]
                        ]
                        td [] [ str story.CitizenName ]
                        td [] [ if story.FromHere then strong [] [ rawText "Yes" ] else rawText "No" ]
                        td [] [ str (fullDate story.RecordedOn tz) ]
                    ])
                |> tbody []
            ]
        ]


/// The page to view a success story
let view (it : Success) citizenName tz =
    article [] [
        h3 [] [
            str citizenName; rawText "&rsquo;s Success Story"
            if it.IsFromHere then
                span [ _class "jjj-heading-label" ] [
                    rawText " &nbsp; &nbsp; "
                    span [ _class "badge bg-success" ] [
                        rawText "Via "
                        rawText (if it.Source = "profile" then "employment profile" else "job listing")
                        rawText " on Jobs, Jobs, Jobs"
                    ]
                ]
        ]
        h4 [ _class "pb-3 text-muted" ] [ str (fullDateTime it.RecordedOn tz) ]
        match it.Story with Some text -> div [] [ md2html text ] | None -> ()
    ]
