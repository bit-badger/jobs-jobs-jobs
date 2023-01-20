module JobsJobsJobs.SuccessStories.Handlers

open System
open Giraffe
open JobsJobsJobs
open JobsJobsJobs.Common.Handlers
open JobsJobsJobs.Domain
open JobsJobsJobs.SuccessStories.Domain

// GET: /success-story/[id]/edit
let edit successId : HttpHandler = requireUser >=> fun next ctx -> task {
    let  citizenId  = currentCitizenId ctx
    let  isNew      = successId = "new"
    let! theSuccess = task {
        if isNew then return Some { Success.empty with CitizenId = citizenId }
        else return! Data.findById (SuccessId.ofString successId)
    }
    match theSuccess with
    | Some success when success.CitizenId = citizenId ->
        let pgTitle = $"""{if isNew then "Tell Your" else "Edit"} Success Story"""
        return!
            Views.edit (EditSuccessForm.fromSuccess success) (success.Id = SuccessId Guid.Empty) pgTitle (csrf ctx)
            |> render pgTitle next ctx
    | Some _ -> return! Error.notAuthorized next ctx
    | None -> return! Error.notFound next ctx
}

// GET: /success-stories
let list : HttpHandler = requireUser >=> fun next ctx -> task {
    let! stories = Data.all ()
    return! Views.list stories (currentCitizenId ctx) (timeZone ctx) |> render "Success Stories" next ctx
}

// GET: /success-story/[id]/view
let view successId : HttpHandler = requireUser >=> fun next ctx -> task {
    // FIXME: make this get both in one query
    match! Data.findById (SuccessId successId) with
    | Some success ->
        match! Citizens.Data.findById success.CitizenId with
        | Some citizen ->
            return! Views.view success (Citizen.name citizen) (timeZone ctx) |> render "Success Story" next ctx
        | None -> return! Error.notFound next ctx
    | None -> return! Error.notFound next ctx
}

// POST: /success-story/save
let save : HttpHandler = requireUser >=> validateCsrf >=> fun next ctx -> task {
    let  citizenId  = currentCitizenId ctx
    let! form       = ctx.BindFormAsync<EditSuccessForm> ()
    let  isNew      = form.Id = ShortGuid.fromGuid Guid.Empty
    let! theSuccess = task {
        if isNew then
            return Some
                    { Success.empty with
                        Id         = SuccessId.create ()
                        CitizenId  = citizenId
                        RecordedOn = now ctx
                        Source     = "profile"
                    }
        else return! Data.findById (SuccessId.ofString form.Id)
    }
    match theSuccess with
    | Some story when story.CitizenId = citizenId ->
        do! Data.save { story with IsFromHere = form.FromHere; Story = noneIfEmpty form.Story |> Option.map Text }
        if isNew then
            match! Profiles.Data.findById citizenId with
            | Some profile -> do! Profiles.Data.save { profile with IsSeekingEmployment = false }
            | None -> ()
        let extraMsg = if isNew then " and seeking employment flag cleared" else ""
        do! addSuccess $"Success story saved{extraMsg} successfully" ctx
        return! redirectToGet "/success-stories" next ctx
    | Some _ -> return! Error.notAuthorized next ctx
    | None -> return! Error.notFound next ctx
}


open Giraffe.EndpointRouting

/// All endpoints for this feature
let endpoints =
    subRoute "/success-stor" [
        GET_HEAD [
            route  "ies"       list
            routef "y/%s/edit" edit
            routef "y/%O/view" view
        ]
        POST [ route "y/save" save ]
    ]
