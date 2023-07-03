module JobsJobsJobs.Listings.Data

open BitBadger.Npgsql.FSharp.Documents
open JobsJobsJobs.Common.Data
open JobsJobsJobs.Domain
open JobsJobsJobs.Listings.Domain
open Npgsql.FSharp

/// The SQL to select a listing view
let viewSql =
    $"SELECT l.*, c.data ->> 'name' AS continent_name, u.data AS cit_data
        FROM {Table.Listing} l
                INNER JOIN {Table.Continent} c ON c.id = l.data ->> 'continentId'
                INNER JOIN {Table.Citizen}   u ON u.id = l.data ->> 'citizenId'"

/// Map a result for a listing view
let private toListingForView row =
    {   Listing       = fromData<Listing> row
        ContinentName = row.string "continent_name"
        Citizen       = fromDocument<Citizen> "cit_data" row
    }

/// Find all job listings posted by the given citizen
let findByCitizen citizenId =
    Custom.list<ListingForView>
        $"{viewSql} WHERE l.data @> @criteria"
        [ "@criteria", Query.jsonbDocParam {| citizenId = CitizenId.toString citizenId |} ]
        toListingForView

/// Find a listing by its ID
let findById listingId =
    Find.byId<Listing> Table.Listing (ListingId.toString listingId)

/// Find a listing by its ID for viewing (includes continent information)
let findByIdForView listingId =
    Custom.single<ListingForView>
        $"{viewSql} WHERE l.id = @id" [ "@id", Sql.string (ListingId.toString listingId) ] toListingForView

/// Save a listing
let save (listing : Listing) =
    save Table.Listing (ListingId.toString listing.Id) listing

/// Search job listings
let search (search : ListingSearchForm) =
    let searches = [
        if search.ContinentId <> "" then
            "l.data @> @continent", [ "@continent", Query.jsonbDocParam {| continentId = search.ContinentId |} ]
        if search.Region <> "" then
            "l.data ->> 'region' ILIKE @region", [ "@region", like search.Region ]
        if search.RemoteWork <> "" then
            "l.data @> @remote", [ "@remote", Query.jsonbDocParam {| isRemote = search.RemoteWork = "yes" |} ]
        if search.Text <> "" then
            "l.data ->> 'text' ILIKE @text", [ "@text", like search.Text ]
    ]
    Custom.list<ListingForView>
        $"""{viewSql}
              WHERE l.data @> '{{ "isExpired": false }}'::jsonb
                {searchSql searches}"""
        (searches |> List.collect snd)
        toListingForView
