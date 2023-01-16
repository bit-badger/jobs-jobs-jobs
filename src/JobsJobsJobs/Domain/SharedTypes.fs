/// Types intended to be shared between the API and the client application
module JobsJobsJobs.Domain.SharedTypes

open JobsJobsJobs.Domain
open Microsoft.Extensions.Options
open NodaTime

/// The data to add or update an other contact
type OtherContactForm =
    {   /// The ID of the contact
        Id : string
        
        /// The type of the contact
        ContactType : string
        
        /// The name of the contact
        Name : string option
        
        /// The value of the contact (URL, e-mail address, phone, etc.)
        Value : string
        
        /// Whether this contact is displayed for public employment profiles and job listings
        IsPublic : bool
    }


/// The data available to update an account profile
type AccountProfileForm =
    {   /// The first name of the citizen
        FirstName : string
        
        /// The last name of the citizen
        LastName : string
        
        /// The display name for the citizen
        DisplayName : string option
        
        /// The citizen's new password
        NewPassword : string option
        
        /// Confirmation of the citizen's new password
        NewPasswordConfirm : string option
        
        /// The contacts for this profile
        Contacts : OtherContactForm list
    }


/// The data required to register a new citizen (user)
type CitizenRegistrationForm =
    {   /// The first name of the new citizen
        FirstName : string
        
        /// The last name of the new citizen
        LastName : string
        
        /// The display name for the new citizen
        DisplayName : string option
        
        /// The citizen's e-mail address
        Email : string
        
        /// The citizen's password
        Password : string
        
        /// Confirmation of the citizen's password
        ConfirmPassword : string
    }


/// The data needed to display a listing
type ListingForView =
    {   /// The listing itself
        Listing : Listing
        
        /// The name of the continent for the listing
        ContinentName : string

        /// The display name of the citizen who owns the listing
        ListedBy : string
    }


/// The various ways job listings can be searched
[<CLIMutable>]
type ListingSearch =
    {   /// Retrieve job listings for this continent
        ContinentId : string option
        
        /// Text for a search within a region
        Region : string option
        
        /// Whether to retrieve job listings for remote work
        RemoteWork : string
        
        /// Text for a search with the job listing description
        Text : string option
    }


/// A successful logon
type LogOnSuccess =
    {   /// The JSON Web Token (JWT) to use for API access
        Jwt : string
        
        /// The ID of the logged-in citizen (as a string)
        CitizenId : string
        
        /// The name of the logged-in citizen
        Name : string
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
[<CLIMutable>]
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


/// The data required to provide a success story
type StoryForm =
    {   /// The ID of this story
        Id : string
        
        /// Whether the employment was obtained from Jobs, Jobs, Jobs
        FromHere : bool
        
        /// The success story
        Story : string
    }


/// An entry in the list of success stories
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
