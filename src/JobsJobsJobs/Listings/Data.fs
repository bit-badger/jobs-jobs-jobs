module JobsJobsJobs.Listings.Data

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
    {   Listing       = toDocument<Listing> row
        ContinentName = row.string "continent_name"
        Citizen       = toDocumentFrom<Citizen> "cit_data" row
    }

/// Find all job listings posted by the given citizen
let findByCitizen citizenId =
    dataSource ()
    |> Sql.query $"{viewSql} WHERE l.data @> @criteria"
    |> Sql.parameters
        [ "@criteria", Sql.jsonb (mkDoc {| citizenId = CitizenId.toString citizenId; isLegacy = false |}) ]
    |> Sql.executeAsync toListingForView

/// Find a listing by its ID
let findById listingId = backgroundTask {
    match! dataSource () |> getDocument<Listing> Table.Listing (ListingId.toString listingId) with
    | Some listing when not listing.IsLegacy -> return Some listing
    | Some _
    | None -> return None
}

/// Find a listing by its ID for viewing (includes continent information)
let findByIdForView listingId = backgroundTask {
    let! tryListing =
        dataSource ()
        |> Sql.query $"""{viewSql} WHERE l.id = @id AND l.data @> '{{ "isLegacy": false }}'::jsonb"""
        |> Sql.parameters [ "@id", Sql.string (ListingId.toString listingId) ]
        |> Sql.executeAsync toListingForView
    return List.tryHead tryListing
}

/// Save a listing
let save (listing : Listing) =
    dataSource () |> saveDocument Table.Listing (ListingId.toString listing.Id) <| mkDoc listing

/// Search job listings
let search (search : ListingSearchForm) =
    let searches = [
        if search.ContinentId <> "" then
            "l.data @> @continent", [ "@continent", Sql.jsonb (mkDoc {| continentId = search.ContinentId |}) ]
        if search.Region <> "" then
            "l.data ->> 'region' ILIKE @region", [ "@region", like search.Region ]
        if search.RemoteWork <> "" then
            "l.data @> @remote", [ "@remote", Sql.jsonb (mkDoc {| isRemote = search.RemoteWork = "yes" |}) ]
        if search.Text <> "" then
            "l.data ->> 'text' ILIKE @text", [ "@text", like search.Text ]
    ]
    dataSource ()
    |> Sql.query $"""
        {viewSql}
         WHERE l.data @> '{{ "isExpired": false, "isLegacy": false }}'::jsonb
           {searchSql searches}"""
    |> Sql.parameters (searches |> List.collect snd)
    |> Sql.executeAsync toListingForView
