module JobsJobsJobs.Profiles.Domain

open JobsJobsJobs.Domain
open NodaTime

/// The data required to update a profile
[<CLIMutable; NoComparison; NoEquality>]
type EditProfileForm =
    {   /// The ID of the continent on which the citizen is located
        ContinentId : string
        
        /// The area within that continent where the citizen is located
        Region : string
        
        /// Whether the citizen to whom this profile belongs is actively seeking employment
        IsSeekingEmployment : bool
        
        /// If the citizen is available for remote work
        RemoteWork : bool
        
        /// If the citizen is seeking full-time employment
        FullTime : bool
        
        /// The user's professional biography
        Biography : string
        
        /// The user's past experience
        Experience : string option
        
        /// The visibility for this profile
        Visibility : string
    }

/// Support functions for the ProfileForm type
module EditProfileForm =

    /// An empty view model (used for new profiles)
    let empty =
        {   ContinentId         = ""
            Region              = ""
            IsSeekingEmployment = false
            RemoteWork          = false
            FullTime            = false
            Biography           = ""
            Experience          = None
            Visibility          = ProfileVisibility.toString Private
        }
    
    /// Create an instance of this form from the given profile
    let fromProfile (profile : Profile) =
        {   ContinentId         = ContinentId.toString profile.ContinentId
            Region              = profile.Region
            IsSeekingEmployment = profile.IsSeekingEmployment
            RemoteWork          = profile.IsRemote
            FullTime            = profile.IsFullTime
            Biography           = MarkdownString.toString profile.Biography
            Experience          = profile.Experience |> Option.map MarkdownString.toString
            Visibility          = ProfileVisibility.toString profile.Visibility
        }


/// The form used to add or edit employment history entries
[<CLIMutable; NoComparison; NoEquality>]
type HistoryForm =
    {   /// The name of the employer
        Employer : string

        StartDate : string

        EndDate : string

        Position : string

        Description : string
    }

/// Support functions for the employment history form
module HistoryForm =

    open System

    /// The date format expected by the browser's date input field
    let dateFormat = Text.LocalDatePattern.CreateWithInvariantCulture "yyyy-MM-dd"

    /// Create an employment history form from an employment history entry
    let fromHistory (history : EmploymentHistory) =
        {   Employer    = history.Employer
            StartDate   = dateFormat.Format history.StartDate
            EndDate     = match history.EndDate with Some dt -> dateFormat.Format dt | None -> ""
            Position    = defaultArg history.Position ""
            Description = match history.Description with Some d -> MarkdownString.toString d | None -> ""
        }
    
    /// Create an employment history entry from an employment history form
    let toHistory (history : HistoryForm) : EmploymentHistory =
        {   Employer    = history.Employer
            StartDate   = (dateFormat.Parse history.StartDate).Value
            EndDate     = if history.EndDate = "" then None else Some (dateFormat.Parse history.EndDate).Value
            Position    = if history.Position = "" then None else Some history.Position
            Description = if history.Description = "" then None else Some (Text history.Description)
        }


/// The various ways profiles can be searched
[<CLIMutable; NoComparison; NoEquality>]
type ProfileSearchForm =
    {   /// Retrieve citizens from this continent
        ContinentId : string
        
        /// Whether to retrieve citizens who do or do not want remote work
        RemoteWork : string
        
        /// Text for a search within a citizen's region, professional bio, skills, experience, and employment history
        Text : string
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


/// The fields required for a skill
[<CLIMutable; NoComparison; NoEquality>]
type SkillForm =
    {   Description : string
        
        /// Notes regarding the skill
        Notes : string
    }

/// Functions to support skill forms
module SkillForm =

    /// Create a skill form from a skill
    let fromSkill (skill : Skill) =
        { SkillForm.Description = skill.Description; Notes = defaultArg skill.Notes "" }
    
    /// Create a skill from a skill form
    let toSkill (form : SkillForm) =
        { Skill.Description = form.Description; Notes = if form.Notes = "" then None else Some form.Notes }
