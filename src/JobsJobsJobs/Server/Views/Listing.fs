/// Views for /profile URLs
[<RequireQualifiedAccess>]
module JobsJobsJobs.Views.Listing

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open JobsJobsJobs.Domain
open JobsJobsJobs.Domain.SharedTypes
open JobsJobsJobs.ViewModels


/// "My Listings" page
let mine (listings : ListingForView list) tz =
    let active  = listings |> List.filter (fun it -> not it.Listing.IsExpired)
    let expired = listings |> List.filter (fun it -> it.Listing.IsExpired)
    article [] [
        h3 [ _class "pb-3" ] [ rawText "My Job Listings" ]
        p [] [ a [ _href "/listing/new/edit"; _class "btn btn-outline-primary" ] [ rawText "Add a New Job Listing" ] ]
        if not (List.isEmpty expired) then h4 [ _class "pb-2" ] [ rawText "Active Job Listings" ]
        if List.isEmpty active then
            p [ _class "pb-3 fst-italic" ] [ rawText "You have no active job listings" ]
        else
            table [ _class "pb-3 table table-sm table-hover pt-3" ] [
                thead [] [
                    [ "Action"; "Title"; "Continent / Region"; "Created"; "Updated" ]
                    |> List.map (fun it -> th [ _scope "col" ] [ rawText it ])
                    |> tr []
                ]
                active
                |> List.map (fun it ->
                    let listId = ListingId.toString it.Listing.Id
                    tr [] [
                        td [] [
                            a [ _href $"/listing/{listId}/edit" ] [ rawText "Edit" ]; rawText " ~ "
                            a [ _href $"/listing/{listId}/view" ] [ rawText "View" ]; rawText " ~ "
                            a [ _href $"/listing/{listId}/expire" ] [ rawText "Expire" ]
                        ]
                        td [] [ str it.Listing.Title ]
                        td [] [ str it.Continent.Name; rawText " / "; str it.Listing.Region ]
                        td [] [ str (fullDateTime it.Listing.CreatedOn tz) ]
                        td [] [ str (fullDateTime it.Listing.UpdatedOn tz) ]
                    ])
                |> tbody []
            ]
        if not (List.isEmpty expired) then
            h4 [ _class "pb-2" ] [ rawText "Expired Job Listings" ]
            table [ _class "table table-sm table-hover pt-3" ] [
                thead [] [
                    [ "Action"; "Title"; "Filled Here?"; "Expired" ]
                    |> List.map (fun it -> th [ _scope "col" ] [ rawText it ])
                    |> tr []
                ]
                expired
                |> List.map (fun it ->
                    tr [] [
                        td [] [ a [ _href $"/listing/{ListingId.toString it.Listing.Id}/view" ] [rawText "View" ] ]
                        td [] [ str it.Listing.Title ]
                        td [] [ str (yesOrNo (defaultArg it.Listing.WasFilledHere false)) ]
                        td [] [ str (fullDateTime it.Listing.UpdatedOn tz) ]
                    ])
                |> tbody []
            ]
    ]
