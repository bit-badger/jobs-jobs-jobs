module JobsJobsJobs.Listings.Handlers

open System
open Giraffe
open JobsJobsJobs
open JobsJobsJobs.Common.Handlers
open JobsJobsJobs.Domain
open JobsJobsJobs.Listings.Domain
open NodaTime

/// Parse the string we receive from JSON into a NodaTime local date
let private parseDate = DateTime.Parse >> LocalDate.FromDateTime

// GET: /listing/[id]/edit
let edit listId : HttpHandler = requireUser >=> fun next ctx -> task {
    let citizenId = currentCitizenId ctx
    let! theListing = task {
        match listId with
        | "new" -> return Some { Listing.empty with CitizenId = citizenId }
        | _ -> return! Data.findById (ListingId.ofString listId)
    }
    match theListing with
    | Some listing when listing.CitizenId = citizenId ->
        let! continents = Common.Data.Continents.all ()
        return!
            Views.edit (EditListingForm.fromListing listing listId) continents (listId = "new") (isHtmx ctx) (csrf ctx)
            |> render $"""{if listId = "new" then "Add a" else "Edit"} Job Listing""" next ctx
    | Some _ -> return! Error.notAuthorized next ctx
    | None -> return! Error.notFound next ctx
}

// GET: /listing/[id]/expire
let expire listingId : HttpHandler = requireUser >=> fun next ctx -> task {
    match! Data.findById (ListingId listingId) with
    | Some listing when listing.CitizenId = currentCitizenId ctx ->
        if listing.IsExpired then
            do! addError $"The listing &ldquo;{listing.Title}&rdquo; is already expired" ctx
            return! redirectToGet "/listings/mine" next ctx
        else
            let form = { Id = ListingId.toString listing.Id; FromHere = false; SuccessStory = "" }
            return! Views.expire form listing (isHtmx ctx) (csrf ctx) |> render "Expire Job Listing" next ctx
    | Some _ -> return! Error.notAuthorized next ctx
    | None -> return! Error.notFound next ctx
}

// POST: /listing/expire
let doExpire : HttpHandler = requireUser >=> validateCsrf >=> fun next ctx -> task {
    let  citizenId = currentCitizenId ctx
    let  now       = now ctx
    let! form      = ctx.BindFormAsync<ExpireListingForm> ()
    match! Data.findById (ListingId.ofString form.Id) with
    | Some listing when listing.CitizenId = citizenId ->
        if listing.IsExpired then
            return! RequestErrors.BAD_REQUEST "Request is already expired" next ctx
        else
            do! Data.save
                    { listing with
                        IsExpired     = true
                        WasFilledHere = Some form.FromHere
                        UpdatedOn     = now
                    }
            if form.SuccessStory <> "" then
                do! SuccessStories.Data.save
                        {   Id         = SuccessId.create()
                            CitizenId  = citizenId
                            RecordedOn = now
                            IsFromHere = form.FromHere
                            Source     = "listing"
                            Story      = (Text >> Some) form.SuccessStory
                        }
            let extraMsg = if form.SuccessStory <> "" then " and success story recorded" else ""
            do! addSuccess $"Job listing expired{extraMsg} successfully" ctx
            return! redirectToGet "/listings/mine" next ctx
    | Some _ -> return! Error.notAuthorized next ctx
    | None -> return! Error.notFound next ctx
}

// GET: /listings/mine
let mine : HttpHandler = requireUser >=> fun next ctx -> task {
    let! listings = Data.findByCitizen (currentCitizenId ctx)
    return! Views.mine listings (timeZone ctx) |> render "My Job Listings" next ctx
}

// POST: /listing/save
let save : HttpHandler = requireUser >=> validateCsrf >=> fun next ctx -> task {
    let  citizenId  = currentCitizenId ctx
    let  now        = now ctx
    let! form       = ctx.BindFormAsync<EditListingForm> ()
    let! theListing = task {
        match form.Id with
        | "new" ->
            return Some
                    { Listing.empty with
                        Id            = ListingId.create ()
                        CitizenId     = currentCitizenId ctx
                        CreatedOn     = now
                        IsExpired     = false
                        WasFilledHere = None
                    }
        | _ -> return! Data.findById (ListingId.ofString form.Id)
    }
    match theListing with
    | Some listing when listing.CitizenId = citizenId ->
        do! Data.save
                { listing with
                    Title       = form.Title
                    ContinentId = ContinentId.ofString form.ContinentId
                    Region      = form.Region
                    IsRemote    = form.RemoteWork
                    Text        = Text form.Text
                    NeededBy    = noneIfEmpty form.NeededBy |> Option.map parseDate
                    UpdatedOn   = now
                }
        do! addSuccess $"""Job listing {if form.Id = "new" then "add" else "updat"}ed successfully""" ctx
        return! redirectToGet $"/listing/{ListingId.toString listing.Id}/edit" next ctx
    | Some _ -> return! Error.notAuthorized next ctx
    | None -> return! Error.notFound next ctx

}

// GET: /help-wanted
let search : HttpHandler = requireUser >=> fun next ctx -> task {
    let! continents = Common.Data.Continents.all ()
    let form =
        match ctx.TryBindQueryString<ListingSearchForm> () with
        | Ok f -> f
        | Error _ -> { ContinentId = ""; Region = ""; RemoteWork = ""; Text = "" }
    let! results = task {
        if string ctx.Request.Query["searched"] = "true" then
            let! it = Data.search form
            return Some it
        else return None
    }
    return! Views.search form continents results |> render "Help Wanted" next ctx
}

// GET: /listing/[id]/view
let view listingId : HttpHandler = requireUser >=> fun next ctx -> task {
    match! Data.findByIdForView (ListingId listingId) with
    | Some listing -> return! Views.view listing |> render $"{listing.Listing.Title} | Job Listing" next ctx
    | None -> return! Error.notFound next ctx
}


open Giraffe.EndpointRouting

/// All endpoints for this feature
let endpoints = [
    GET_HEAD [ route "/help-wanted" search ]
    subRoute "/listing" [
        GET_HEAD [
            route  "s/mine"     mine
            routef "/%s/edit"   edit
            routef "/%O/expire" expire
            routef "/%O/view"   view
        ]
        POST [
            route "/expire" doExpire
            route "/save"   save
        ]
    ]
]
