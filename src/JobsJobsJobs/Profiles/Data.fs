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

/// Convert a data row to a profile for viewing
let private toProfileForView row =
    {   Profile   = toDocument<Profile> row
        Citizen   = toDocumentFrom<Citizen>   "cit_data"  row
        Continent = toDocumentFrom<Continent> "cont_data" row
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
        |> Sql.executeAsync toProfileForView
    return List.tryHead tryCitizen
}

/// Save a profile
let save (profile : Profile) =
    dataSource () |> saveDocument Table.Profile (CitizenId.toString profile.Id) <| mkDoc profile

/// Search profiles
let search (search : ProfileSearchForm) isPublic = backgroundTask {
    let searches = [
        if search.ContinentId <> "" then
            "p.data ->> 'continentId' = @continentId", [ "@continentId", Sql.string search.ContinentId ]
        if search.RemoteWork <> "" then
            "p.data ->> 'isRemote' = @remote", [ "@remote", jsonBool (search.RemoteWork = "yes") ]
        if search.Text <> "" then
            "p.text_search @@ plainto_tsquery(@text_search)", [ "@text_search", Sql.string search.Text ]
    ]
    let vizSql =
        if isPublic then
            sprintf "IN ('%s', '%s')" (ProfileVisibility.toString Public) (ProfileVisibility.toString Anonymous)
        else sprintf "<> '%s'" (ProfileVisibility.toString Hidden)
    let! results =
        dataSource ()
        |> Sql.query $"
            SELECT p.*, c.data AS cit_data, o.data AS cont_data
                FROM {Table.Profile} p
                    INNER JOIN {Table.Citizen}   c ON c.id = p.id
                    INNER JOIN {Table.Continent} o ON o.id = p.data ->> 'continentId'
                WHERE p.data ->> 'isLegacy' = 'false'
                  AND p.data ->> 'visibility' {vizSql}
                {searchSql searches}"
        |> Sql.parameters (searches |> List.collect snd)
        |> Sql.executeAsync toProfileForView
    return results |> List.sortBy (fun pfv -> (Citizen.name pfv.Citizen).ToLowerInvariant ())
}
