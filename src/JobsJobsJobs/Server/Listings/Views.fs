/// Views for /profile URLs
module JobsJobsJobs.Listings.Views

open Giraffe.ViewEngine
open JobsJobsJobs.Common.Views
open JobsJobsJobs.Domain
open JobsJobsJobs.Listings.Domain


/// Job listing edit page
let edit (m : EditListingForm) continents isNew csrf =
    pageWithTitle $"""{if isNew then "Add a" else "Edit"} Job Listing""" [
        form [ _class "row g-3"; _method "POST"; _action "/listing/save" ] [
            antiForgery csrf
            input [ _type "hidden"; _name (nameof m.Id); _value m.Id ]
            div [ _class "col-12 col-sm-10 col-md-8 col-lg-6" ] [
                textBox [ _type "text"; _maxlength "255"; _autofocus ] (nameof m.Title) m.Title "Title" true
                div [ _class "form-text" ] [
                    txt "No need to put location here; it will always be show to seekers with continent and region"
                ]
            ]
            div [ _class "col-12 col-sm-6 col-md-4" ] [
                continentList [] (nameof m.ContinentId) continents None m.ContinentId true
            ]
            div [ _class "col-12 col-sm-6 col-md-8" ] [
                textBox [ _type "text"; _maxlength "255" ] (nameof m.Region) m.Region "Region" true
                div [ _class "form-text" ] [ txt "Country, state, geographic area, etc." ]
            ]
            div [ _class "col-12" ] [
                checkBox [] (nameof m.RemoteWork) m.RemoteWork "This opportunity is for remote work"
            ]
            markdownEditor [ _required ] (nameof m.Text) m.Text "Job Description"
            div [ _class "col-12 col-md-4" ] [
                textBox [ _type "date" ] (nameof m.NeededBy) m.NeededBy "Needed By" false
            ]
            div [ _class "col-12" ] [ submitButton "content-save-outline" "Save" ]
        ]
    ]


open System.Net

/// Page to expire a job listing
let expire (m : ExpireListingForm) (listing : Listing) csrf =
    pageWithTitle $"Expire Job Listing ({WebUtility.HtmlEncode listing.Title})" [
        p [ _class "fst-italic" ] [
            txt "Expiring this listing will remove it from search results. You will be able to see it via your "
            txt "&ldquo;My Job Listings&rdquo; page, but you will not be able to &ldquo;un-expire&rdquo; it."
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
                    txt "Consider telling your fellow citizens about your experience! Comments entered here will be "
                    txt "visible to logged-on users here, but not to the general public."
                ]
            ]
            markdownEditor [] (nameof m.SuccessStory) m.SuccessStory "Your Success Story"
            div [ _class "col-12" ] [ submitButton "text-box-remove-outline" "Expire Listing" ]
        ]
        jsOnLoad "jjj.listing.toggleFromHere()"
    ]


/// "My Listings" page
let mine (listings : ListingForView list) tz =
    let active  = listings |> List.filter (fun it -> not it.Listing.IsExpired)
    let expired = listings |> List.filter (fun it -> it.Listing.IsExpired)
    pageWithTitle "My Job Listings" [
        p [] [ a [ _href "/listing/new/edit"; _class "btn btn-outline-primary" ] [ txt "Add a New Job Listing" ] ]
        if not (List.isEmpty expired) then h4 [ _class "pb-2" ] [ txt "Active Job Listings" ]
        if List.isEmpty active then p [ _class "pb-3 fst-italic" ] [ txt "You have no active job listings" ]
        else
            table [ _class "pb-3 table table-sm table-hover pt-3" ] [
                thead [] [
                    [ "Action"; "Title"; "Continent / Region"; "Created"; "Updated" ]
                    |> List.map (fun it -> th [ _scope "col" ] [ txt it ])
                    |> tr []
                ]
                active
                |> List.map (fun it ->
                    let listId = ListingId.toString it.Listing.Id
                    tr [] [
                        td [] [
                            a [ _href $"/listing/{listId}/edit" ] [ txt "Edit" ]; txt " ~ "
                            a [ _href $"/listing/{listId}/view" ] [ txt "View" ]; txt " ~ "
                            a [ _href $"/listing/{listId}/expire" ] [ txt "Expire" ]
                        ]
                        td [] [ str it.Listing.Title ]
                        td [] [ str it.ContinentName; rawText " / "; str it.Listing.Region ]
                        td [] [ str (fullDateTime it.Listing.CreatedOn tz) ]
                        td [] [ str (fullDateTime it.Listing.UpdatedOn tz) ]
                    ])
                |> tbody []
            ]
        if not (List.isEmpty expired) then
            h4 [ _class "pb-2" ] [ txt "Expired Job Listings" ]
            table [ _class "table table-sm table-hover pt-3" ] [
                thead [] [
                    [ "Action"; "Title"; "Filled Here?"; "Expired" ]
                    |> List.map (fun it -> th [ _scope "col" ] [ txt it ])
                    |> tr []
                ]
                expired
                |> List.map (fun it ->
                    tr [] [
                        td [] [ a [ _href $"/listing/{ListingId.toString it.Listing.Id}/view" ] [ txt "View" ] ]
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
    pageWithTitle "Help Wanted" [
        if Option.isNone listings then
            p [] [
                txt "Enter relevant criteria to find results, or just click &ldquo;Search&rdquo; to see all active job "
                txt "listings."
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
                        textBox [ _maxlength "1000" ] (nameof m.Text) m.Text "Job Listing Text" false
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
        match listings with
        | Some r when List.isEmpty r ->
            p [ _class "pt-3" ] [ txt "No job listings found for the specified criteria" ]
        | Some r ->
            table [ _class "table table-sm table-hover pt-3" ] [
                thead [] [
                    tr [] [
                        th [ _scope "col" ] [ txt "Listing" ]
                        th [ _scope "col" ] [ txt "Title" ]
                        th [ _scope "col" ] [ txt "Location" ]
                        th [ _scope "col"; _class "text-center" ] [ txt "Remote?" ]
                        th [ _scope "col"; _class "text-center" ] [ txt "Needed By" ]
                    ]
                ]
                r |> List.map (fun it ->
                    tr [] [
                        td [] [ a [ _href $"/listing/{ListingId.toString it.Listing.Id}/view" ] [ txt "View" ] ]
                        td [] [ str it.Listing.Title ]
                        td [] [ str it.ContinentName; rawText " / "; str it.Listing.Region ]
                        td [ _class "text-center" ] [ str (yesOrNo it.Listing.IsRemote) ]
                        td [ _class "text-center" ] [
                            match it.Listing.NeededBy with Some needed -> str (neededBy needed) | None -> txt "N/A"
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
                    txt " &nbsp; &nbsp; "; span [ _class "badge bg-warning text-dark" ] [ txt "Expired" ]
                    if defaultArg it.Listing.WasFilledHere false then
                        txt " &nbsp; &nbsp; "; span [ _class "badge bg-success" ] [ txt "Filled via Jobs, Jobs, Jobs" ]
                ]
        ]
        h4 [ _class "pb-3 text-muted" ] [ str it.ContinentName; rawText " / "; str it.Listing.Region ]
        p [] [
            match it.Listing.NeededBy with
            | Some needed ->
                strong [] [ em [] [ txt "NEEDED BY "; str ((neededBy needed).ToUpperInvariant ()) ] ]; txt " &bull; "
            | None -> ()
            txt "Listed by "; strong [ _class "me-4" ] [ str (Citizen.name it.Citizen) ]; br []
            span [ _class "ms-3" ] []; yield! contactInfo it.Citizen false
        ]
        hr []
        div [] [ md2html it.Listing.Text ]
    ]
