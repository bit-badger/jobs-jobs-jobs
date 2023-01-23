/// Views for /success-stor[y|ies] URLs
module JobsJobsJobs.SuccessStories.Views

open Giraffe.ViewEngine
open JobsJobsJobs.Common.Views
open JobsJobsJobs.Domain
open JobsJobsJobs.SuccessStories.Domain

/// The add/edit success story page
let edit (m : EditSuccessForm) isNew pgTitle isHtmx csrf =
    pageWithTitle pgTitle [
        if isNew then
            p [] [
                txt "Congratulations on your employment! Your fellow citizens would enjoy hearing how it all came "
                txt "about; tell us about it below! "
                em [] [ txt "(These will be visible to other users, but not to the general public.)" ]
            ]
        form [ _class "row g-3"; _method "POST"; _action "/success-story/save" ] [
            antiForgery csrf
            input [ _type "hidden"; _name (nameof m.Id); _value m.Id ]
            div [ _class "col-12" ] [
                checkBox [] (nameof m.FromHere) m.FromHere "I found my employment here"
            ]
            markdownEditor [] (nameof m.Story) m.Story "The Success Story" isHtmx
            div [ _class "col-12" ] [
                submitButton "content-save-outline" "Save"
                if isNew then
                    p [ _class "fst-italic" ] [
                        txt "(Saving this will set &ldquo;Seeking Employment&rdquo; to &ldquo;No&rdquo; on your "
                        txt "profile.)"
                    ]
            ]
        ]
    ]


/// The list of success stories
let list (m : StoryEntry list) citizenId tz =
    pageWithTitle "Success Stories" [
        if List.isEmpty m then
            p [] [ txt "There are no success stories recorded "; em [] [ txt "(yet)" ] ]
        else
            table [ _class "table table-sm table-hover" ] [
                thead [] [
                    [ "Story"; "From"; "Found Here?"; "Recorded On" ]
                    |> List.map (fun it -> th [ _scope "col" ] [ txt it ])
                    |> tr []
                ]
                m |> List.map (fun story ->
                    tr [] [
                        td [] [
                            let theId = SuccessId.toString story.Id
                            if story.HasStory then a [ _href $"/success-story/{theId}/view" ] [ txt "View" ]
                            else em [] [ txt "None" ]
                            if story.CitizenId = citizenId then
                                txt " ~ "; a [ _href $"/success-story/{theId}/edit" ] [ txt "Edit" ]
                        ]
                        td [] [ str story.CitizenName ]
                        td [] [ if story.FromHere then strong [] [ txt "Yes" ] else txt "No" ]
                        td [] [ str (fullDate story.RecordedOn tz) ]
                    ])
                |> tbody []
            ]
        ]


/// The page to view a success story
let view (it : Success) citizenName tz =
    article [] [
        h3 [] [
            str citizenName; txt "&rsquo;s Success Story"
            if it.IsFromHere then
                span [ _class "jjj-heading-label" ] [
                    txt " &nbsp; &nbsp; "
                    span [ _class "badge bg-success" ] [
                        txt "Via "; txt (if it.Source = "profile" then "employment profile" else "job listing")
                        txt " on Jobs, Jobs, Jobs"
                    ]
                ]
        ]
        h4 [ _class "pb-3 text-muted" ] [ str (fullDateTime it.RecordedOn tz) ]
        match it.Story with Some text -> div [] [ md2html text ] | None -> ()
    ]
