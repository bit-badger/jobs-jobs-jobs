module JobsJobsJobs.Profiles.Data

open JobsJobsJobs.Common.Data
open JobsJobsJobs.Domain
open JobsJobsJobs.Profiles.Domain
open Npgsql.FSharp

/// Count the current profiles
let count () =
    dataSource ()
    |> Sql.query $"SELECT COUNT(id) AS the_count FROM {Table.Profile} WHERE data ->> 'isLegacy' = 'false'"
    |> Sql.executeRowAsync (fun row -> row.int64 "the_count")

/// Delete a profile by its ID
let deleteById citizenId = backgroundTask {
    let! _ =
        dataSource ()
        |> Sql.query $"DELETE FROM {Table.Profile} WHERE id = @id"
        |> Sql.parameters [ "@id", Sql.string (CitizenId.toString citizenId) ]
        |> Sql.executeNonQueryAsync
    ()
}

/// Find a profile by citizen ID
let findById citizenId = backgroundTask {
    match! dataSource () |> getDocument<Profile> Table.Profile (CitizenId.toString citizenId) with
    | Some profile when not profile.IsLegacy -> return Some profile
    | Some _
    | None -> return None
}

/// Find a profile by citizen ID for viewing (includes citizen and continent information)
let findByIdForView citizenId = backgroundTask {
    let! tryCitizen =
        dataSource ()
        |> Sql.query $"
            SELECT p.*, c.data AS cit_data, o.data AS cont_data
                FROM {Table.Profile} p
                    INNER JOIN {Table.Citizen}   c ON c.id = p.id
                    INNER JOIN {Table.Continent} o ON o.id = p.data ->> 'continentId'
                WHERE p.id                  = @id
                AND p.data ->> 'isLegacy' = 'false'"
        |> Sql.parameters [ "@id", Sql.string (CitizenId.toString citizenId) ]
        |> Sql.executeAsync (fun row ->
            {   Profile   = toDocument<Profile> row
                Citizen   = toDocumentFrom<Citizen>   "cit_data"  row
                Continent = toDocumentFrom<Continent> "cont_data" row
            })
    return List.tryHead tryCitizen
}

/// Save a profile
let save (profile : Profile) =
    dataSource () |> saveDocument Table.Profile (CitizenId.toString profile.Id) <| mkDoc profile

/// Search profiles (logged-on users)
let search (search : ProfileSearchForm) = backgroundTask {
    let searches = [
        if search.ContinentId <> "" then
            "p.data ->> 'continentId' = @continentId", [ "@continentId", Sql.string search.ContinentId ]
        if search.RemoteWork <> "" then
            "p.data ->> 'isRemote' = @remote", [ "@remote", jsonBool (search.RemoteWork = "yes") ]
        if search.Text <> "" then
            "   p.data ->> 'region'     ILIKE @text
             OR p.data ->> 'biography'  ILIKE @text
             OR p.data ->> 'experience' ILIKE @text
             OR EXISTS (
                    SELECT 1 FROM jsonb_array_elements(p.data['skills']) x(elt)
                     WHERE x ->> 'description' ILIKE @text)
             OR EXISTS (
                    SELECT 1 FROM jsonb_array_elements(p.data['history']) x(elt)
                     WHERE x ->> 'employer'    ILIKE @text
                        OR x ->> 'position'    ILIKE @text
                        OR x ->> 'description' ILIKE @text)",
            [ "@text", like search.Text ]
    ]
    let! results =
        dataSource ()
        |> Sql.query $"
            SELECT p.*, c.data AS cit_data
                FROM {Table.Profile} p
                    INNER JOIN {Table.Citizen} c ON c.id = p.id
                WHERE p.data ->> 'isLegacy'    = 'false'
                  AND p.data ->> 'visibility' <> '{ProfileVisibility.toString Hidden}'
                {searchSql searches}"
        |> Sql.parameters (searches |> List.collect snd)
        |> Sql.executeAsync (fun row ->
            let profile = toDocument<Profile> row
            let citizen = toDocumentFrom<Citizen> "cit_data" row
            {   CitizenId         = profile.Id
                DisplayName       = Citizen.name citizen
                SeekingEmployment = profile.IsSeekingEmployment
                RemoteWork        = profile.IsRemote
                FullTime          = profile.IsFullTime
                LastUpdatedOn     = profile.LastUpdatedOn
            })
    return results |> List.sortBy (fun psr -> psr.DisplayName.ToLowerInvariant ())
}

// Search profiles (public)
let publicSearch (search : PublicSearchForm) =
    let searches = [
        if search.ContinentId <> "" then
            "p.data ->> 'continentId' = @continentId", [ "@continentId", Sql.string search.ContinentId ]
        if search.Region <> "" then
            "p.data ->> 'region' ILIKE @region", [ "@region", like search.Region ]
        if search.RemoteWork <> "" then
            "p.data ->> 'isRemote' = @remote", [ "@remote", jsonBool (search.RemoteWork = "yes") ]
        if search.Skill <> "" then
            "EXISTS (
                SELECT 1 FROM jsonb_array_elements(p.data['skills']) x(elt)
                    WHERE x ->> 'description' ILIKE @description)",
            [ "@description", like search.Skill ]
    ]
    dataSource ()
    |> Sql.query $"
        SELECT p.*, c.data AS cont_data
            FROM {Table.Profile} p
                INNER JOIN {Table.Continent} c ON c.id = p.data ->> 'continentId'
            WHERE p.data ->> 'isPubliclySearchable' = 'true'
            AND p.data ->> 'isLegacy'             = 'false'
            {searchSql searches}"
    |> Sql.parameters (searches |> List.collect snd)
    |> Sql.executeAsync (fun row ->
        let profile = toDocument<Profile> row
        let continent = toDocumentFrom<Continent> "cont_data" row
        {   Continent  = continent.Name
            Region     = profile.Region
            RemoteWork = profile.IsRemote
            Skills     = profile.Skills
                            |> List.map (fun s ->
                                let notes = match s.Notes with Some n -> $" ({n})" | None -> ""
                                $"{s.Description}{notes}")
        })
