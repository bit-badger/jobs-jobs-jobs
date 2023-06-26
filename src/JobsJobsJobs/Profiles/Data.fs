module JobsJobsJobs.Profiles.Data

open BitBadger.Npgsql.FSharp.Documents
open JobsJobsJobs.Common.Data
open JobsJobsJobs.Domain
open JobsJobsJobs.Profiles.Domain
open Npgsql.FSharp

/// Count the current profiles
let count () =
    Count.all Table.Profile

/// Delete a profile by its ID
let deleteById citizenId =
    Delete.byId Table.Profile (CitizenId.toString citizenId)

/// Find a profile by citizen ID
let findById citizenId =
    Find.byId<Profile> Table.Profile (CitizenId.toString citizenId)

/// Convert a data row to a profile for viewing
let private toProfileForView row =
    {   Profile   = fromData<Profile> row
        Citizen   = fromDocument<Citizen>   "cit_data"  row
        Continent = fromDocument<Continent> "cont_data" row
    }

/// Find a profile by citizen ID for viewing (includes citizen and continent information)
let findByIdForView citizenId =
    Custom.single<ProfileForView>
        $"SELECT p.*, c.data AS cit_data, o.data AS cont_data
            FROM {Table.Profile} p
                 INNER JOIN {Table.Citizen}   c ON c.id = p.id
                 INNER JOIN {Table.Continent} o ON o.id = p.data ->> 'continentId'
           WHERE p.id = @id"
        [ "@id", Sql.string (CitizenId.toString citizenId) ]
        toProfileForView

/// Save a profile
let save (profile : Profile) =
    save Table.Profile (CitizenId.toString profile.Id) profile

/// Search profiles
let search (search : ProfileSearchForm) isPublic = backgroundTask {
    let searches = [
        if search.ContinentId <> "" then
            "p.data @> @continent", [ "@continent", Query.jsonbDocParam {| continentId = search.ContinentId |} ]
        if search.RemoteWork <> "" then
            "p.data @> @remote", [ "@remote", Query.jsonbDocParam {| isRemote = search.RemoteWork = "yes" |} ]
        if search.Text <> "" then
            "p.text_search @@ plainto_tsquery(@text_search)", [ "@text_search", Sql.string search.Text ]
    ]
    let vizSql =
        if isPublic then
            sprintf "(p.data @> '%s'::jsonb OR p.data @> '%s'::jsonb)"
                (Configuration.serializer().Serialize {| visibility = ProfileVisibility.toString Public |})
                (Configuration.serializer().Serialize {| visibility = ProfileVisibility.toString Anonymous |})
        else sprintf "p.data ->> 'visibility' <> '%s'" (ProfileVisibility.toString Hidden)
    let! results =
        Custom.list<ProfileForView>
            $" SELECT p.*, c.data AS cit_data, o.data AS cont_data
                 FROM {Table.Profile} p
                      INNER JOIN {Table.Citizen}   c ON c.id = p.id
                      INNER JOIN {Table.Continent} o ON o.id = p.data ->> 'continentId'
                WHERE {vizSql}
                  {searchSql searches}"
            (searches |> List.collect snd)
            toProfileForView
    return results |> List.sortBy (fun pfv -> (Citizen.name pfv.Citizen).ToLowerInvariant ())
}
