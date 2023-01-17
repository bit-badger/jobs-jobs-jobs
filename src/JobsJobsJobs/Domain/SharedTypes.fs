/// Types intended to be shared between the API and the client application
module JobsJobsJobs.Domain.SharedTypes

open JobsJobsJobs.Domain
open NodaTime

/// The data needed to display a listing
[<NoComparison; NoEquality>]
type ListingForView =
    {   /// The listing itself
        Listing : Listing
        
        /// The name of the continent for the listing
        ContinentName : string

        /// The display name of the citizen who owns the listing
        ListedBy : string
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


/// The various ways profiles can be searched
[<CLIMutable; NoComparison; NoEquality>]
type ProfileSearchForm =
    {   /// Retrieve citizens from this continent
        ContinentId : string
        
        /// Text for a search within a citizen's skills
        Skill : string
        
        /// Text for a search with a citizen's professional biography and experience fields
        BioExperience : string
        
        /// Whether to retrieve citizens who do or do not want remote work
        RemoteWork : string
    }


/// A user matching the profile search
[<NoComparison; NoEquality>]
type ProfileSearchResult =
    {   /// The ID of the citizen
        CitizenId : CitizenId
        
        /// The citizen's display name
        DisplayName : string
        
        /// Whether this citizen is currently seeking employment
        SeekingEmployment : bool
        
        /// Whether this citizen is looking for remote work
        RemoteWork : bool
        
        /// Whether this citizen is looking for full-time work
        FullTime : bool
        
        /// When this profile was last updated
        LastUpdatedOn : Instant
    }


/// The data required to show a viewable profile
type ProfileForView =
    {   /// The profile itself
        Profile : Profile
        
        /// The citizen to whom the profile belongs
        Citizen : Citizen
        
        /// The continent for the profile
        Continent : Continent
    }


/// The parameters for a public job search
[<CLIMutable; NoComparison; NoEquality>]
type PublicSearchForm =
    {   /// Retrieve citizens from this continent
        ContinentId : string
        
        /// Retrieve citizens from this region
        Region : string
        
        /// Text for a search within a citizen's skills
        Skill : string
        
        /// Whether to retrieve citizens who do or do not want remote work
        RemoteWork : string
    }


/// A public profile search result
[<NoComparison; NoEquality>]
type PublicSearchResult =
    {   /// The name of the continent on which the citizen resides
        Continent : string
        
        /// The region in which the citizen resides
        Region : string
        
        /// Whether this citizen is seeking remote work
        RemoteWork : bool
        
        /// The skills this citizen has identified
        Skills : string list
    }


/// An entry in the list of success stories
[<NoComparison; NoEquality>]
type StoryEntry =
    {   /// The ID of this success story
        Id : SuccessId
        
        /// The ID of the citizen who recorded this story
        CitizenId : CitizenId
        
        /// The name of the citizen who recorded this story
        CitizenName : string
        
        /// When this story was recorded
        RecordedOn : Instant
        
        /// Whether this story involves an opportunity that arose due to Jobs, Jobs, Jobs
        FromHere : bool
        
        /// Whether this report has a further story, or if it is simply a "found work" entry
        HasStory : bool
    }
