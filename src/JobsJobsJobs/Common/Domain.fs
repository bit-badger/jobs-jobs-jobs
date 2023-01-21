namespace JobsJobsJobs.Domain

open System
open Giraffe
open NodaTime

// ~~~ SUPPORT TYPES ~~~ //

/// The ID of a user (a citizen of Gitmo Nation)
type CitizenId = CitizenId of Guid

/// Support functions for citizen IDs
module CitizenId =
    
    /// Create a new citizen ID
    let create () = (Guid.NewGuid >> CitizenId) ()
    
    /// A string representation of a citizen ID
    let toString = function CitizenId it -> ShortGuid.fromGuid it
    
    /// Parse a string into a citizen ID
    let ofString = ShortGuid.toGuid >> CitizenId
    
    /// Get the GUID value of a citizen ID
    let value = function CitizenId guid -> guid


/// Types of contacts supported by Jobs, Jobs, Jobs
type ContactType =
    /// E-mail addresses
    | Email
    /// Phone numbers (home, work, cell, etc.)
    | Phone
    /// Websites (personal, social, etc.)
    | Website

/// Functions to support contact types
module ContactType =
    
    /// Parse a contact type from a string
    let parse typ =
        match typ with
        | "Email" -> Email
        | "Phone" -> Phone
        | "Website" -> Website
        | it -> invalidOp $"{it} is not a valid contact type"
    
    /// Convert a contact type to its string representation
    let toString =
        function
        | Email -> "Email"
        | Phone -> "Phone"
        | Website -> "Website"


/// The ID of a continent
type ContinentId = ContinentId of Guid

/// Support functions for continent IDs
module ContinentId =
    
    /// Create a new continent ID
    let create () = (Guid.NewGuid >> ContinentId) ()
    
    /// A string representation of a continent ID
    let toString = function ContinentId it -> ShortGuid.fromGuid it
    
    /// Parse a string into a continent ID
    let ofString = ShortGuid.toGuid >> ContinentId
    
    /// Get the GUID value of a continent ID
    let value = function ContinentId guid -> guid


/// A string of Markdown text
type MarkdownString = Text of string

/// Support functions for Markdown strings
module MarkdownString =
    
    open Markdig
    
    /// The Markdown conversion pipeline (enables all advanced features)
    let private pipeline = MarkdownPipelineBuilder().UseAdvancedExtensions().Build ()
    
    /// Convert this Markdown string to HTML
    let toHtml = function Text text -> Markdown.ToHtml (text, pipeline)
    
    /// Convert a Markdown string to its string representation
    let toString = function Text text -> text


/// An employment history entry
[<NoComparison; NoEquality>]
type EmploymentHistory =
    {   /// The employer for this period of employment
        Employer : string

        /// The date employment started
        StartDate : LocalDate

        /// The date employment ended (None implies ongoing employment)
        EndDate : LocalDate option

        /// The title / position held
        Position : string option

        /// A description of the duties entailed during this employment
        Description : MarkdownString option
    }

/// Support functions for employment history entries
module EmploymentHistory =

    let empty =
        {   Employer    = ""
            StartDate   = LocalDate.FromDateTime DateTime.Today
            EndDate     = None
            Position    = None
            Description = None
        }


/// The ID of a job listing
type ListingId = ListingId of Guid

/// Support functions for listing IDs
module ListingId =
    
    /// Create a new job listing ID
    let create () = (Guid.NewGuid >> ListingId) ()
    
    /// A string representation of a listing ID
    let toString = function ListingId it -> ShortGuid.fromGuid it
    
    /// Parse a string into a listing ID
    let ofString = ShortGuid.toGuid >> ListingId
    
    /// Get the GUID value of a listing ID
    let value = function ListingId guid -> guid


/// Another way to contact a citizen from this site 
[<NoComparison; NoEquality>]
type OtherContact =
    {   /// The type of contact
        ContactType : ContactType
        
        /// The name of the contact (Email, No Agenda Social, LinkedIn, etc.) 
        Name : string option
        
        /// The value for the contact (e-mail address, user name, URL, etc.)
        Value : string
        
        /// Whether this contact is visible in public employment profiles and job listings
        IsPublic : bool
    }


/// Visibility options for an employment profile
type ProfileVisibility =
    /// Profile is only visible to authenticated users
    | Private
    /// Anonymous information is visible to public users
    | Anonymous
    /// The full employment profile is visible to public users
    | Public

/// Support functions for profile visibility
module ProfileVisibility =

    /// Parse a string into a profile visibility
    let parse viz =
        match viz with
        | "Private" -> Private
        | "Anonymous" -> Anonymous
        | "Public" -> Public
        | it -> invalidOp $"{it} is not a valid profile visibility value"
    
    /// Convert a profile visibility to its string representation
    let toString =
        function
        | Private -> "Private"
        | Anonymous -> "Anonymous"
        | Public -> "Public"


/// A skill the job seeker possesses
[<NoComparison; NoEquality>]
type Skill =
    {   /// A description of the skill
        Description : string
        
        /// Notes regarding this skill (level, duration, etc.)
        Notes : string option
    }


/// The ID of a success report
type SuccessId = SuccessId of Guid

/// Support functions for success report IDs
module SuccessId =
    
    /// Create a new success report ID
    let create () = (Guid.NewGuid >> SuccessId) ()
    
    /// A string representation of a success report ID
    let toString = function SuccessId it -> ShortGuid.fromGuid it
    
    /// Parse a string into a success report ID
    let ofString = ShortGuid.toGuid >> SuccessId
    
    /// Get the GUID value of a success ID
    let value = function SuccessId guid -> guid

// ~~~ DOCUMENT TYPES ~~~ //

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
[<NoComparison; NoEquality>]
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
        
        /// The ID of the continent on which the citizen resides
        ContinentId : ContinentId
        
        /// The region in which the citizen resides
        Region : string
        
        /// Whether this citizen is actively seeking employment
        IsSeekingEmployment : bool
        
        /// Whether the citizen is interested in remote work
        IsRemote : bool
        
        /// Whether the citizen is interested in full-time work
        IsFullTime : bool
        
        /// The citizen's professional biography
        Biography : MarkdownString
        
        /// Skills this citizen possesses
        Skills : Skill list
        
        /// The citizen's employment history
        History : EmploymentHistory list

        /// The citizen's experience (topical / chronological)
        Experience : MarkdownString option
        
        /// The visibility of this profile
        Visibility : ProfileVisibility
        
        /// When the citizen last updated their profile
        LastUpdatedOn : Instant
        
        /// Whether this is a legacy profile
        IsLegacy : bool
    }

/// Support functions for Profiles
module Profile =
    
    // An empty profile
    let empty = {
        Id                  = CitizenId Guid.Empty
        ContinentId         = ContinentId Guid.Empty
        Region              = ""
        IsSeekingEmployment = false
        IsRemote            = false
        IsFullTime          = false
        Biography           = Text ""
        Skills              = []
        History             = []
        Experience          = None
        Visibility          = Private
        LastUpdatedOn       = Instant.MinValue
        IsLegacy            = false
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
