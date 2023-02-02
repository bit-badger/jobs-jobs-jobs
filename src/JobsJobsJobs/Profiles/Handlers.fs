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
    let  citizenId = currentCitizenId ctx
    let! profile   = Data.findById citizenId
    let  display   = match profile with Some p -> p | None -> { Profile.empty with Id = citizenId }
    return! Views.edit display |> render "Employment Profile" next ctx
}

// GET: /profile/edit/general
let editGeneralInfo : HttpHandler = requireUser >=> fun next ctx -> task {
    let! profile    = Data.findById (currentCitizenId ctx)
    let! continents = Common.Data.Continents.all ()
    let  form       = if Option.isNone profile then EditProfileForm.empty else EditProfileForm.fromProfile profile.Value
    return!
        Views.editGeneralInfo form continents (isHtmx ctx) (csrf ctx)
        |> render "General Information | Employment Profile" next ctx
}

// POST: /profile/save
let saveGeneralInfo : HttpHandler = requireUser >=> fun next ctx -> task {
    let  citizenId = currentCitizenId ctx
    let! form      = ctx.BindFormAsync<EditProfileForm> ()
    let  errors    = [
        if form.ContinentId = "" then "Continent is required"
        if form.Region      = "" then "Region is required"
        if form.Biography   = "" then "Professional Biography is required"
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
                    ContinentId         = ContinentId.ofString form.ContinentId
                    Region              = form.Region
                    IsSeekingEmployment = form.IsSeekingEmployment
                    IsRemote            = form.RemoteWork
                    IsFullTime          = form.FullTime
                    Biography           = Text form.Biography
                    LastUpdatedOn       = now ctx
                    Experience          = noneIfBlank form.Experience |> Option.map Text
                    Visibility          = ProfileVisibility.parse form.Visibility
                }
        let action = if isNew then "cre" else "upd"
        do! addSuccess $"Employment Profile {action}ated successfully" ctx
        return! redirectToGet "/profile/edit" next ctx
    else
        do! addErrors errors ctx
        let! continents = Common.Data.Continents.all ()
        return!
            Views.editGeneralInfo form continents (isHtmx ctx) (csrf ctx)
            |> render "General Information | Employment Profile" next ctx
}

// GET: /profile/search
let search : HttpHandler = fun next ctx -> task {
    let! continents = Common.Data.Continents.all ()
    let form =
        match ctx.TryBindQueryString<ProfileSearchForm> () with
        | Ok f -> f
        | Error _ -> { ContinentId = ""; RemoteWork = ""; Text = "" }
    let isPublic = tryUser ctx |> Option.isNone
    let! results = task {
        if string ctx.Request.Query["searched"] = "true" then
            let! it = Data.search form isPublic
            return Some it
        else return None
    }
    return! Views.search form continents (timeZone ctx) results isPublic |> render "Profile Search" next ctx
}

// GET: /profile/edit/skills
let skills : HttpHandler = requireUser >=> fun next ctx -> task {
    match! Data.findById (currentCitizenId ctx) with
    | Some profile -> return! Views.skills profile.Skills (csrf ctx) |> render "Skills | Employment Profile" next ctx
    | None -> return! notFound ctx
}

// GET: /profile/edit/skills/list
let skillList : HttpHandler = requireUser >=> fun next ctx -> task {
    match! Data.findById (currentCitizenId ctx) with
    | Some profile -> return! Views.skillTable profile.Skills None (csrf ctx) |> renderBare next ctx
    | None -> return! notFound ctx
}

// GET: /profile/edit/skill/[idx]
let editSkill idx : HttpHandler = requireUser >=> fun next ctx -> task {
    match! Data.findById (currentCitizenId ctx) with
    | Some profile ->
        if idx < -1 || idx >= List.length profile.Skills then return! notFound ctx
        else return! Views.editSkill profile.Skills idx (csrf ctx) |> renderBare next ctx
    | None -> return! notFound ctx
}

// POST: /profile/edit/skill/[idx]
let saveSkill idx : HttpHandler = requireUser >=> validateCsrf >=> fun next ctx -> task {
    match! Data.findById (currentCitizenId ctx) with
    | Some profile ->
        if idx < -1 || idx >= List.length profile.Skills then return! notFound ctx
        else
            let! form  = ctx.BindFormAsync<SkillForm> ()
            let  skill = SkillForm.toSkill form
            let  skills =
                if idx = -1 then skill :: profile.Skills
                else profile.Skills |> List.mapi (fun skillIdx it -> if skillIdx = idx then skill else it)
                |> List.sortBy (fun it -> it.Description.ToLowerInvariant ())
            do! Data.save { profile with Skills = skills }
            return! Views.skillTable skills None (csrf ctx) |> renderBare next ctx
    | None -> return! notFound ctx
}

// POST: /profile/edit/skill/[idx]/delete
let deleteSkill idx : HttpHandler = requireUser >=> validateCsrf >=> fun next ctx -> task {
    match! Data.findById (currentCitizenId ctx) with
    | Some profile ->
        if idx < 0 || idx >= List.length profile.Skills then return! notFound ctx
        else
            let skills = profile.Skills |> List.indexed |> List.filter (fun it -> fst it <> idx) |> List.map snd
            do! Data.save { profile with Skills = skills }
            return! Views.skillTable skills None (csrf ctx) |> renderBare next ctx
    | None -> return! notFound ctx
}

// GET: /profile/edit/history
let history : HttpHandler = requireUser >=> fun next ctx -> task {
    match! Data.findById (currentCitizenId ctx) with
    | Some profile ->
        return!
            Views.history profile.History (isHtmx ctx) (csrf ctx)
            |> render "Employment History | Employment Profile" next ctx
    | None -> return! notFound ctx
}

// GET: /profile/edit/history/list
let historyList : HttpHandler = requireUser >=> fun next ctx -> task {
    match! Data.findById (currentCitizenId ctx) with
    | Some profile -> return! Views.historyTable profile.History None (isHtmx ctx) (csrf ctx) |> renderBare next ctx
    | None -> return! notFound ctx
}

// GET: /profile/edit/history/[idx]
let editHistory idx : HttpHandler = requireUser >=> fun next ctx -> task {
    match! Data.findById (currentCitizenId ctx) with
    | Some profile ->
        if idx < -1 || idx >= List.length profile.History then return! notFound ctx
        else return! Views.editHistory profile.History idx (isHtmx ctx) (csrf ctx) |> renderBare next ctx
    | None -> return! notFound ctx
}

// POST: /profile/edit/history/[idx]
let saveHistory idx : HttpHandler = requireUser >=> validateCsrf >=> fun next ctx -> task {
    match! Data.findById (currentCitizenId ctx) with
    | Some profile ->
        if idx < -1 || idx >= List.length profile.History then return! notFound ctx
        else
            let! form    = ctx.BindFormAsync<HistoryForm> ()
            let  entry   = HistoryForm.toHistory form
            let  history =
                if idx = -1 then entry :: profile.History
                else profile.History |> List.mapi (fun histIdx it -> if histIdx = idx then entry else it)
                |> List.sortByDescending (fun it -> defaultArg it.EndDate NodaTime.LocalDate.MaxIsoValue)
            do! Data.save { profile with History = history }
            return! Views.historyTable history None (isHtmx ctx) (csrf ctx) |> renderBare next ctx
    | None -> return! notFound ctx
}

// POST: /profile/edit/history/[idx]/delete
let deleteHistory idx : HttpHandler = requireUser >=> validateCsrf >=> fun next ctx -> task {
    match! Data.findById (currentCitizenId ctx) with
    | Some profile ->
        if idx < 0 || idx >= List.length profile.History then return! notFound ctx
        else
            let history = profile.History |> List.indexed |> List.filter (fun it -> fst it <> idx) |> List.map snd
            do! Data.save { profile with History = history }
            return! Views.historyTable history None (isHtmx ctx) (csrf ctx) |> renderBare next ctx
    | None -> return! notFound ctx
}

/// Get a profile for view, and enforce visibility restrictions against the current user
let private getProfileForView citizenId ctx = task {
    let citId = CitizenId citizenId
    match! Data.findByIdForView citId with
    | Some profile ->
        let currentCitizen = tryUser ctx |> Option.map CitizenId.ofString
        let canView =
            match profile.Profile.Visibility, currentCitizen with
            | Private, Some _
            | Anonymous, Some _
            | Public, _ -> true
            | Hidden, Some citizenId when profile.Citizen.Id = citizenId -> true
            | _ -> false
        return if canView then Some (profile, currentCitizen) else None
    | None -> return None
}

// GET: /profile/[id]/view
let view citizenId : HttpHandler = fun next ctx -> task {
    match! getProfileForView citizenId ctx with
    | Some (profile, currentCitizen) ->
        let title = $"Employment Profile for {Citizen.name profile.Citizen}"
        return! Views.view profile currentCitizen |> render title next ctx
    | None -> return! notFound ctx
}

// GET: /profile/[id]/print
let print citizenId : HttpHandler = fun next ctx -> task {
    match! getProfileForView citizenId ctx with
    | Some (profile, currentCitizen) ->
        let pageTitle = $"Employment Profile for {Citizen.name profile.Citizen}"
        return! Views.print profile (Option.isNone currentCitizen) |> renderPrint pageTitle next ctx
    | None -> return! notFound ctx
}


open Giraffe.EndpointRouting

/// All endpoints for this feature
let endpoints =
    subRoute "/profile" [
        GET_HEAD [
            routef "/%O/view"           view
            routef "/%O/print"          print
            route  "/edit"              edit
            route  "/edit/general"      editGeneralInfo
            routef "/edit/history/%i"   editHistory
            route  "/edit/history"      history
            route  "/edit/history/list" historyList
            routef "/edit/skill/%i"     editSkill
            route  "/edit/skills"       skills
            route  "/edit/skills/list"  skillList
            route  "/search"            search
        ]
        POST [
            route  "/delete"                 delete
            routef "/edit/history/%i"        saveHistory
            routef "/edit/history/%i/delete" deleteHistory
            routef "/edit/skill/%i"          saveSkill
            routef "/edit/skill/%i/delete"   deleteSkill
            route  "/save"                   saveGeneralInfo
        ]
    ]
