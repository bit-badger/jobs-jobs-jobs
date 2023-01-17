/// Views for /profile URLs
[<RequireQualifiedAccess>]
module JobsJobsJobs.Views.Listing

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open JobsJobsJobs.Domain
open JobsJobsJobs.Domain.SharedTypes
open JobsJobsJobs.ViewModels


/// Job listing edit page
let edit (m : EditListingForm) continents isNew csrf =
    article [] [
        h3 [ _class "pb-3" ] [ rawText (if isNew then "Add a" else "Edit"); rawText " Job Listing" ]
        form [ _class "row g-3"; _method "POST"; _action "/listing/save" ] [
            antiForgery csrf
            input [ _type "hidden"; _name (nameof m.Id); _value m.Id ]
            div [ _class "col-12 col-sm-10 col-md-8 col-lg-6" ] [
                textBox [ _type "text"; _maxlength "255"; _autofocus ] (nameof m.Title) m.Title "Title" true
                div [ _class "form-text" ] [
                    rawText "No need to put location here; it will always be show to seekers with continent and region"
                ]
            ]
            div [ _class "col-12 col-sm-6 col-md-4" ] [
                continentList [] (nameof m.ContinentId) continents None m.ContinentId true
            ]
            div [ _class "col-12 col-sm-6 col-md-8" ] [
                textBox [ _type "text"; _maxlength "255" ] (nameof m.Region) m.Region "Region" true
                div [ _class "form-text" ] [ rawText "Country, state, geographic area, etc." ]
            ]
            div [ _class "col-12" ] [
                checkBox [] (nameof m.RemoteWork) m.RemoteWork "This opportunity is for remote work"
            ]
            markdownEditor [ _required ] (nameof m.Text) m.Text "Job Description"
            div [ _class "col-12 col-md-4" ] [
                textBox [ _type "date" ] (nameof m.NeededBy) m.NeededBy "Needed By" false
            ]
            div [ _class "col-12" ] [
                button [ _type "submit"; _class "btn btn-primary" ] [
                    i [ _class "mdi mdi-content-save-outline" ] []; rawText "&nbsp; Save"
                ]
            ]
        ]
    ]


let expire (m : ExpireListingForm) (listing : Listing) csrf =
    article [] [
        h3 [ _class "pb-3" ] [ rawText "Expire Job Listing ("; str listing.Title; rawText ")" ]
        p [ _class "fst-italic" ] [
            rawText "Expiring this listing will remove it from search results. You will be able to see it via your "
            rawText "&ldquo;My Job Listings&rdquo; page, but you will not be able to &ldquo;un-expire&rdquo; it."
        ]
        form [ _class "row g-3"; _method "POST"; _action "/listing/expire" ] [
            antiForgery csrf
            input [ _type "hidden"; _name (nameof m.Id); _value m.Id ]
            div [ _class "col-12" ] [
                checkBox [ _onclick "jjj.listing.toggleFromHere()" ] (nameof m.FromHere) m.FromHere
                         "This job was filled due to its listing here"
            ]
            div [ _class "col-12"; _id "successRow" ] [
                p [] [
                    rawText "Consider telling your fellow citizens about your experience! Comments entered here will "
                    rawText "be visible to logged-on users here, but not to the general public."
                ]
            ]
            markdownEditor [] (nameof m.SuccessStory) m.SuccessStory "Your Success Story"
            div [ _class "col-12" ] [
                button [ _type "submit"; _class "btn btn-primary" ] [
                    i [ _class "mdi mdi-text-box-remove-outline" ] []; rawText "&nbsp; Expire Listing"
                ]
            ]
        ]
        jsOnLoad "jjj.listing.toggleFromHere()"
    ]


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
                        td [] [ str it.ContinentName; rawText " / "; str it.Listing.Region ]
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

open NodaTime.Text

/// Format the needed by date
let private neededBy dt = 
    (LocalDatePattern.CreateWithCurrentCulture "MMMM d, yyyy").Format dt

let search (m : ListingSearchForm) continents (listings : ListingForView list option) =
    article [] [
        h3 [ _class "pb-3" ] [ rawText "Help Wanted" ]
        if Option.isNone listings then
            p [] [
                rawText "Enter relevant criteria to find results, or just click &ldquo;Search&rdquo; to see all "
                rawText "current job listings."
            ]
        collapsePanel "Search Criteria" [
            form [ _class "container"; _method "GET"; _action "/help-wanted" ] [
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
                        textBox [ _maxlength "1000" ] (nameof m.Text) m.Text "Job Listing Text" false
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
        match listings with
        | Some r when List.isEmpty r ->
            p [ _class "pt-3" ] [ rawText "No job listings found for the specified criteria" ]
        | Some r ->
            table [ _class "table table-sm table-hover pt-3" ] [
                thead [] [
                    tr [] [
                        th [ _scope "col" ] [ rawText "Listing" ]
                        th [ _scope "col" ] [ rawText "Title" ]
                        th [ _scope "col" ] [ rawText "Location" ]
                        th [ _scope "col"; _class "text-center" ] [ rawText "Remote?" ]
                        th [ _scope "col"; _class "text-center" ] [ rawText "Needed By" ]
                    ]
                ]
                r |> List.map (fun it ->
                    tr [] [
                        td [] [ a [ _href $"/listing/{ListingId.toString it.Listing.Id}/view" ] [ rawText "View" ] ]
                        td [] [ str it.Listing.Title ]
                        td [] [ str it.ContinentName; rawText " / "; str it.Listing.Region ]
                        td [ _class "text-center" ] [ str (yesOrNo it.Listing.IsRemote) ]
                        td [ _class "text-center" ] [
                            match it.Listing.NeededBy with Some needed -> str (neededBy needed) | None -> rawText "N/A"
                        ]
                    ])
                |> tbody []
            ]
        | None -> ()
    ]

/// The job listing view page
let view (it : ListingForView) =
    article [] [
        h3 [] [
            str it.Listing.Title
            if it.Listing.IsExpired then
                span [ _class "jjj-heading-label" ] [
                    rawText " &nbsp; &nbsp; "; span [ _class "badge bg-warning text-dark" ] [ rawText "Expired" ]
                    if defaultArg it.Listing.WasFilledHere false then
                        rawText " &nbsp; &nbsp; "
                        span [ _class "badge bg-success" ] [ rawText "Filled via Jobs, Jobs, Jobs" ]
                ]
        ]
        h4 [ _class "pb-3 text-muted" ] [ str it.ContinentName; rawText " / "; str it.Listing.Region ]
        p [] [
            match it.Listing.NeededBy with
            | Some needed ->
                strong [] [ em [] [ rawText "NEEDED BY "; str ((neededBy needed).ToUpperInvariant ()) ] ]
                rawText " &bull; "
            | None -> ()
            rawText "Listed by "; strong [ _class "me-4" ] [ str (Citizen.name it.Citizen) ]; br []
            span [ _class "ms-3" ] []; yield! contactInfo it.Citizen false
        ]
        hr []
        div [] [ md2html it.Listing.Text ]
    ]
