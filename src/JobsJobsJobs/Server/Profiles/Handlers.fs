module JobsJobsJobs.Profiles.Handlers

open Giraffe
open JobsJobsJobs
open JobsJobsJobs.Common.Handlers
open JobsJobsJobs.Domain
open JobsJobsJobs.Profiles.Domain

// POST: /profile/delete
let delete : HttpHandler = requireUser >=> validateCsrf >=> fun next ctx -> task {
    do! Data.deleteById (currentCitizenId ctx)
    do! addSuccess "Profile deleted successfully" ctx
    return! redirectToGet "/citizen/dashboard" next ctx
}

// GET: /profile/edit
let edit : HttpHandler = requireUser >=> fun next ctx -> task {
    let  citizenId  = currentCitizenId ctx
    let! profile    = Data.findById citizenId
    let! continents = Common.Data.Continents.all ()
    let  isNew      = Option.isNone profile
    let  form       = if isNew then EditProfileForm.empty else EditProfileForm.fromProfile profile.Value
    let  title      = $"""{if isNew then "Create" else "Edit"} Profile"""
    return! Views.edit form continents isNew citizenId (csrf ctx) |> render title next ctx
}

// POST: /profile/save
let save : HttpHandler = requireUser >=> fun next ctx -> task {
    let  citizenId = currentCitizenId ctx
    let! theForm   = ctx.BindFormAsync<EditProfileForm> ()
    let  form      = { theForm with Skills = theForm.Skills |> Array.filter (box >> isNull >> not) }
    let  errors    = [
        if form.ContinentId = "" then "Continent is required"
        if form.Region      = "" then "Region is required"
        if form.Biography   = "" then "Professional Biography is required"
        if form.Skills |> Array.exists (fun s -> s.Description = "") then "All skill Descriptions are required"
    ]
    let! profile = task {
        match! Data.findById citizenId with
        | Some p -> return p
        | None -> return { Profile.empty with Id = citizenId }
    }
    let isNew = profile.Region = ""
    if List.isEmpty errors then
        do! Data.save
                { profile with
                    IsSeekingEmployment  = form.IsSeekingEmployment
                    ContinentId          = ContinentId.ofString form.ContinentId
                    Region               = form.Region
                    IsRemote             = form.RemoteWork
                    IsFullTime           = form.FullTime
                    Biography            = Text form.Biography
                    LastUpdatedOn        = now ctx
                    Skills               = form.Skills
                                            |> Array.filter (fun s -> (box >> isNull >> not) s)
                                            |> Array.map SkillForm.toSkill
                                            |> List.ofArray
                    Experience           = noneIfBlank form.Experience |> Option.map Text
                    IsPubliclySearchable = form.IsPubliclySearchable
                    IsPubliclyLinkable   = form.IsPubliclyLinkable
                }
        let action = if isNew then "cre" else "upd"
        do! addSuccess $"Employment Profile {action}ated successfully" ctx
        return! redirectToGet "/profile/edit" next ctx
    else
        do! addErrors errors ctx
        let! continents = Common.Data.Continents.all ()
        return!
            Views.edit form continents isNew citizenId (csrf ctx)
            |> render $"""{if isNew then "Create" else "Edit"} Profile""" next ctx
}

// GET: /profile/search
let search : HttpHandler = requireUser >=> fun next ctx -> task {
    let! continents = Common.Data.Continents.all ()
    let form =
        match ctx.TryBindQueryString<ProfileSearchForm> () with
        | Ok f -> f
        | Error _ -> { ContinentId = ""; RemoteWork = ""; Skill = ""; BioExperience = "" }
    let! results = task {
        if string ctx.Request.Query["searched"] = "true" then
            let! it = Data.search form
            return Some it
        else return None
    }
    return! Views.search form continents (timeZone ctx) results |> render "Profile Search" next ctx
}

// GET: /profile/seeking
let seeking : HttpHandler = fun next ctx -> task {
    let! continents = Common.Data.Continents.all ()
    let form =
        match ctx.TryBindQueryString<PublicSearchForm> () with
        | Ok f -> f
        | Error _ -> { ContinentId = ""; Region = ""; RemoteWork = ""; Skill = "" }
    let! results = task {
        if string ctx.Request.Query["searched"] = "true" then
            let! it = Data.publicSearch form
            return Some it
        else return None
    }
    return! Views.publicSearch form continents results |> render "Profile Search" next ctx
}

// GET: /profile/[id]/view
let view citizenId : HttpHandler = fun next ctx -> task {
    let citId = CitizenId citizenId
    match! Data.findByIdForView citId with
    | Some profile ->
        let currentCitizen = tryUser ctx |> Option.map CitizenId.ofString
        if not profile.Profile.IsPubliclyLinkable && Option.isNone currentCitizen then
            return! Error.notAuthorized next ctx
        else
            let title = $"Employment Profile for {Citizen.name profile.Citizen}"
            return! Views.view profile currentCitizen |> render title next ctx
    | None -> return! Error.notFound next ctx
}


open Giraffe.EndpointRouting

/// All endpoints for this feature
let endpoints =
    subRoute "/profile" [
        GET_HEAD [
            routef "/%O/view" view
            route  "/edit"    edit
            route  "/search"  search
            route  "/seeking" seeking
        ]
        POST [
            route "/delete" delete
            route "/save"   save
        ]
    ]
