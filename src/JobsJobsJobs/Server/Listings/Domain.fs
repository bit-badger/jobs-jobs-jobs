module JobsJobsJobs.Listings.Domain

open JobsJobsJobs.Domain

/// The data required to add or edit a job listing
[<CLIMutable; NoComparison; NoEquality>]
type EditListingForm =
    {   /// The ID of the listing
        Id : string
        
        /// The listing title
        Title : string
        
        /// The ID of the continent on which this opportunity exists
        ContinentId : string
        
        /// The region in which this opportunity exists
        Region : string
        
        /// Whether this is a remote work opportunity
        RemoteWork : bool
        
        /// The text of the job listing
        Text : string
        
        /// The date by which this job listing is needed
        NeededBy : string
    }

/// Support functions to support listings
module EditListingForm =

    open NodaTime.Text

    /// Create a listing form from an existing listing
    let fromListing (listing : Listing) theId =
        let neededBy =
            match listing.NeededBy with
            | Some dt -> (LocalDatePattern.CreateWithCurrentCulture "yyyy-MM-dd").Format dt
            | None -> ""
        {   Id          = theId
            Title       = listing.Title
            ContinentId = ContinentId.toString listing.ContinentId
            Region      = listing.Region
            RemoteWork  = listing.IsRemote
            Text        = MarkdownString.toString listing.Text
            NeededBy    = neededBy
        }


/// The form submitted to expire a listing
[<CLIMutable; NoComparison; NoEquality>]
type ExpireListingForm =
    {   /// The ID of the listing to expire
        Id : string
        
        /// Whether the job was filled from here
        FromHere : bool
        
        /// The success story written by the user
        SuccessStory : string
    }


/// The data needed to display a listing
[<NoComparison; NoEquality>]
type ListingForView =
    {   /// The listing itself
        Listing : Listing
        
        /// The name of the continent for the listing
        ContinentName : string

        /// The citizen who owns the listing
        Citizen : Citizen
    }


/// The various ways job listings can be searched
[<CLIMutable; NoComparison; NoEquality>]
type ListingSearchForm =
    {   /// Retrieve job listings for this continent
        ContinentId : string
        
        /// Text for a search within a region
        Region : string
        
        /// Whether to retrieve job listings for remote work
        RemoteWork : string
        
        /// Text for a search with the job listing description
        Text : string
    }

