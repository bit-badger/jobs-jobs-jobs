namespace JobsJobsJobs.Domain

open NodaTime
open System

/// A user of Jobs, Jobs, Jobs; a citizen of Gitmo Nation
[<NoComparison; NoEquality>]
type Citizen =
    {   /// The ID of the user
        Id : CitizenId
        
        /// When the user joined Jobs, Jobs, Jobs
        JoinedOn : Instant
        
        /// When the user last logged in
        LastSeenOn : Instant
        
        /// The user's e-mail address
        Email : string
        
        /// The user's first name
        FirstName : string
        
        /// The user's last name
        LastName : string
        
        /// The hash of the user's password
        PasswordHash : string
        
        /// The name displayed for this user throughout the site
        DisplayName : string option
        
        /// The other contacts for this user
        OtherContacts : OtherContact list
        
        /// Whether this is a legacy citizen
        IsLegacy : bool
    }

/// Support functions for citizens
module Citizen =
    
    /// An empty citizen
    let empty = {
        Id            = CitizenId Guid.Empty
        JoinedOn      = Instant.MinValue
        LastSeenOn    = Instant.MinValue
        Email         = ""
        FirstName     = ""
        LastName      = ""
        PasswordHash  = ""
        DisplayName   = None
        OtherContacts = []
        IsLegacy      = false
    }
    
    /// Get the name of the citizen (either their preferred display name or first/last names)
    let name x =
        match x.DisplayName with Some it -> it | None -> $"{x.FirstName} {x.LastName}"


/// A continent
[<NoComparison; NoEquality>]
type Continent =
    {   /// The ID of the continent
        Id : ContinentId
        
        /// The name of the continent
        Name : string
    }

/// Support functions for continents
module Continent =
    
    /// An empty continent
    let empty ={
        Id   = ContinentId Guid.Empty
        Name = ""
    }


/// A job listing
[<NoComparison; NoEquality>]
type Listing =
    {   /// The ID of the job listing
        Id : ListingId
        
        /// The ID of the citizen who posted the job listing
        CitizenId : CitizenId
        
        /// When this job listing was created
        CreatedOn : Instant
        
        /// The short title of the job listing
        Title : string
        
        /// The ID of the continent on which the job is located
        ContinentId : ContinentId
        
        /// The region in which the job is located
        Region : string
        
        /// Whether this listing is for remote work
        IsRemote : bool
        
        /// Whether this listing has expired
        IsExpired : bool
        
        /// When this listing was last updated
        UpdatedOn : Instant
        
        /// The details of this job
        Text : MarkdownString
        
        /// When this job needs to be filled
        NeededBy : LocalDate option
        
        /// Was this job filled as part of its appearance on Jobs, Jobs, Jobs?
        WasFilledHere : bool option
        
        /// Whether this is a legacy listing
        IsLegacy : bool
    }

/// Support functions for job listings
module Listing =
    
    /// An empty job listing
    let empty = {
        Id            = ListingId Guid.Empty
        CitizenId     = CitizenId Guid.Empty
        CreatedOn     = Instant.MinValue
        Title         = ""
        ContinentId   = ContinentId Guid.Empty
        Region        = ""
        IsRemote      = false
        IsExpired     = false
        UpdatedOn     = Instant.MinValue
        Text          = Text ""
        NeededBy      = None
        WasFilledHere = None
        IsLegacy      = false
    }


/// Security settings for a user
type SecurityInfo =
    {   /// The ID of the citizen to whom these settings apply
        Id : CitizenId
        
        /// The number of failed log on attempts (reset to 0 on successful log on)
        FailedLogOnAttempts : int
        
        /// Whether the account is locked
        AccountLocked : bool
        
        /// The token the user must provide to take their desired action
        Token : string option
        
        /// The action to which the token applies
        TokenUsage : string option
        
        /// When the token expires
        TokenExpires : Instant option
    }

/// Functions to support security info
module SecurityInfo =
    
    /// An empty set of security info
    let empty = {
        Id                  = CitizenId Guid.Empty
        FailedLogOnAttempts = 0
        AccountLocked       = false
        Token               = None
        TokenUsage          = None
        TokenExpires        = None
    }


/// A job seeker profile
[<NoComparison; NoEquality>]
type Profile =
    {   /// The ID of the citizen to whom this profile belongs
        Id : CitizenId
        
        /// Whether this citizen is actively seeking employment
        IsSeekingEmployment : bool
        
        /// Whether this citizen allows their profile to be a part of the publicly-viewable, anonymous data
        IsPubliclySearchable : bool
        
        /// Whether this citizen allows their profile to be viewed via a public link
        IsPubliclyLinkable : bool
        
        /// The ID of the continent on which the citizen resides
        ContinentId : ContinentId
        
        /// The region in which the citizen resides
        Region : string
        
        /// Whether the citizen is looking for remote work
        IsRemote : bool
        
        /// Whether the citizen is looking for full-time work
        IsFullTime : bool
        
        /// The citizen's professional biography
        Biography : MarkdownString
        
        /// When the citizen last updated their profile
        LastUpdatedOn : Instant
        
        /// The citizen's experience (topical / chronological)
        Experience : MarkdownString option
        
        /// Skills this citizen possesses
        Skills : Skill list
        
        /// Whether this is a legacy profile
        IsLegacy : bool
    }

/// Support functions for Profiles
module Profile =
    
    // An empty profile
    let empty = {
        Id                   = CitizenId Guid.Empty
        IsSeekingEmployment  = false
        IsPubliclySearchable = false
        IsPubliclyLinkable   = false
        ContinentId          = ContinentId Guid.Empty
        Region               = ""
        IsRemote             = false
        IsFullTime           = false
        Biography            = Text ""
        LastUpdatedOn        = Instant.MinValue
        Experience           = None
        Skills               = []
        IsLegacy             = false
    }


/// A record of success finding employment
[<NoComparison; NoEquality>]
type Success =
    {   /// The ID of the success report
        Id : SuccessId
        
        /// The ID of the citizen who wrote this success report
        CitizenId : CitizenId
        
        /// When this success report was recorded
        RecordedOn : Instant
        
        /// Whether the success was due, at least in part, to Jobs, Jobs, Jobs
        IsFromHere : bool
        
        /// The source of this success (listing or profile)
        Source : string
        
        /// The success story
        Story : MarkdownString option
    }

/// Support functions for success stories
module Success =
    
    /// An empty success story
    let empty = {
        Id         = SuccessId Guid.Empty
        CitizenId  = CitizenId Guid.Empty
        RecordedOn = Instant.MinValue
        IsFromHere   = false
        Source     = ""
        Story      = None
    }
