namespace JobsJobsJobs.Domain

open NodaTime
open System

// fsharplint:disable FieldNames

/// A user of Jobs, Jobs, Jobs; a citizen of Gitmo Nation
[<CLIMutable; NoComparison; NoEquality>]
type Citizen =
    {   /// The ID of the user
        id : CitizenId
        
        /// When the user joined Jobs, Jobs, Jobs
        joinedOn : Instant
        
        /// When the user last logged in
        lastSeenOn : Instant
        
        /// The user's e-mail address
        email : string
        
        /// The user's first name
        firstName : string
        
        /// The user's last name
        lastName : string
        
        /// The hash of the user's password
        passwordHash : string
        
        /// The name displayed for this user throughout the site
        displayName : string option
        
        /// The other contacts for this user
        otherContacts : OtherContact list
        
    }

/// Support functions for citizens
module Citizen =
    
    /// Get the name of the citizen (either their preferred display name or first/last names)
    let name x =
        match x.displayName with Some it -> it | None -> $"{x.firstName} {x.lastName}"


/// A continent
[<CLIMutable; NoComparison; NoEquality>]
type Continent =
    {   /// The ID of the continent
        id   : ContinentId
        
        /// The name of the continent
        name : string
    }


/// A job listing
[<CLIMutable; NoComparison; NoEquality>]
type Listing =
    {   /// The ID of the job listing
        id            : ListingId
        
        /// The ID of the citizen who posted the job listing
        citizenId     : CitizenId
        
        /// When this job listing was created
        createdOn     : Instant
        
        /// The short title of the job listing
        title         : string
        
        /// The ID of the continent on which the job is located
        continentId   : ContinentId
        
        /// The region in which the job is located
        region        : string
        
        /// Whether this listing is for remote work
        remoteWork    : bool
        
        /// Whether this listing has expired
        isExpired     : bool
        
        /// When this listing was last updated
        updatedOn     : Instant
        
        /// The details of this job
        text          : MarkdownString
        
        /// When this job needs to be filled
        neededBy      : LocalDate option
        
        /// Was this job filled as part of its appearance on Jobs, Jobs, Jobs?
        wasFilledHere : bool option
    }


/// A skill the job seeker possesses
type Skill =
    {   /// The ID of the skill
        id          : SkillId
        
        /// A description of the skill
        description : string
        
        /// Notes regarding this skill (level, duration, etc.)
        notes       : string option
    }


/// A job seeker profile
[<CLIMutable; NoComparison; NoEquality>]
type Profile =
    {   /// The ID of the citizen to whom this profile belongs
        id                : CitizenId
        
        /// Whether this citizen is actively seeking employment
        seekingEmployment : bool
        
        /// Whether this citizen allows their profile to be a part of the publicly-viewable, anonymous data
        isPublic          : bool
        
        /// Whether this citizen allows their profile to be viewed via a public link
        isPublicLinkable  : bool
        
        /// The ID of the continent on which the citizen resides
        continentId       : ContinentId
        
        /// The region in which the citizen resides
        region            : string
        
        /// Whether the citizen is looking for remote work
        remoteWork        : bool
        
        /// Whether the citizen is looking for full-time work
        fullTime          : bool
        
        /// The citizen's professional biography
        biography         : MarkdownString
        
        /// When the citizen last updated their profile
        lastUpdatedOn     : Instant
        
        /// The citizen's experience (topical / chronological)
        experience        : MarkdownString option
        
        /// Skills this citizen possesses
        skills            : Skill list
    }

/// Support functions for Profiles
module Profile =
    
    // An empty profile
    let empty =
        { id                = CitizenId Guid.Empty
          seekingEmployment = false
          isPublic          = false
          isPublicLinkable  = false
          continentId       = ContinentId Guid.Empty
          region            = ""
          remoteWork        = false
          fullTime          = false
          biography         = Text ""
          lastUpdatedOn     = Instant.MinValue
          experience        = None
          skills            = []
        }


/// A record of success finding employment
[<CLIMutable; NoComparison; NoEquality>]
type Success =
    {   /// The ID of the success report
        id         : SuccessId
        
        /// The ID of the citizen who wrote this success report
        citizenId  : CitizenId
        
        /// When this success report was recorded
        recordedOn : Instant
        
        /// Whether the success was due, at least in part, to Jobs, Jobs, Jobs
        fromHere   : bool
        
        /// The source of this success (listing or profile)
        source     : string
        
        /// The success story
        story      : MarkdownString option
    }
