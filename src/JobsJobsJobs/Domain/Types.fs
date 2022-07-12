/// Types within Jobs, Jobs, Jobs
module JobsJobsJobs.Domain.Types

open NodaTime
open System

// fsharplint:disable FieldNames

/// The ID of a user (a citizen of Gitmo Nation)
type CitizenId = CitizenId of Guid

/// A user of Jobs, Jobs, Jobs
[<CLIMutable; NoComparison; NoEquality>]
type Citizen =
    {   /// The ID of the user
        id           : CitizenId
        /// The Mastodon instance abbreviation from which this citizen is authorized
        instance     : string
        /// The handle by which the user is known on Mastodon
        mastodonUser : string
        /// The user's display name from Mastodon (updated every login)
        displayName  : string option
        /// The user's real name
        realName     : string option
        /// The URL for the user's Mastodon profile
        profileUrl   : string
        /// When the user joined Jobs, Jobs, Jobs
        joinedOn     : Instant
        /// When the user last logged in
        lastSeenOn   : Instant
    }


/// The ID of a continent
type ContinentId = ContinentId of Guid

/// A continent
[<CLIMutable; NoComparison; NoEquality>]
type Continent =
    {   /// The ID of the continent
        id   : ContinentId
        /// The name of the continent
        name : string
    }


/// A string of Markdown text
type MarkdownString = Text of string


/// The ID of a job listing
type ListingId = ListingId of Guid

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


/// The ID of a skill
type SkillId = SkillId of Guid

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

/// The ID of a success report
type SuccessId = SuccessId of Guid

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
