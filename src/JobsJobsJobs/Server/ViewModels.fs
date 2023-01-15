/// View models for Jobs, Jobs, Jobs
module JobsJobsJobs.ViewModels

open JobsJobsJobs.Domain

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


/// The data required to update a profile
[<CLIMutable; NoComparison; NoEquality>]
type EditProfileViewModel =
    {   /// Whether the citizen to whom this profile belongs is actively seeking employment
        IsSeekingEmployment : bool
        
        /// The ID of the continent on which the citizen is located
        ContinentId : string
        
        /// The area within that continent where the citizen is located
        Region : string
        
        /// If the citizen is available for remote work
        RemoteWork : bool
        
        /// If the citizen is seeking full-time employment
        FullTime : bool
        
        /// The user's professional biography
        Biography : string
        
        /// The skills for the user
        Skills : SkillForm array

        /// The user's past experience
        Experience : string option
        
        /// Whether this profile should appear in the public search
        IsPubliclySearchable : bool
        
        /// Whether this profile should be shown publicly
        IsPubliclyLinkable : bool
    }

/// Support functions for the ProfileForm type
module EditProfileViewModel =

    /// An empty view model (used for new profiles)
    let empty =
        {   IsSeekingEmployment  = false
            ContinentId          = ""
            Region               = ""
            RemoteWork           = false
            FullTime             = false
            Biography            = ""
            Skills               = [||]
            Experience           = None
            IsPubliclySearchable = false
            IsPubliclyLinkable   = false
        }
    
    /// Create an instance of this form from the given profile
    let fromProfile (profile : Profile) =
        {   IsSeekingEmployment  = profile.IsSeekingEmployment
            ContinentId          = ContinentId.toString profile.ContinentId
            Region               = profile.Region
            RemoteWork           = profile.IsRemote
            FullTime             = profile.IsFullTime
            Biography            = MarkdownString.toString profile.Biography
            Skills               = profile.Skills |> List.map SkillForm.fromSkill |> Array.ofList
            Experience           = profile.Experience |> Option.map MarkdownString.toString
            IsPubliclySearchable = profile.IsPubliclySearchable
            IsPubliclyLinkable   = profile.IsPubliclyLinkable
        }


/// View model for the log on page
[<CLIMutable; NoComparison; NoEquality>]
type LogOnViewModel =
    {   /// A message regarding an error encountered during a log on attempt
        ErrorMessage : string option

        /// The e-mail address for the user attempting to log on
        Email : string

        /// The password of the user attempting to log on
        Password : string

        /// The URL where the user should be redirected after logging on
        ReturnTo : string option
    }


/// View model for the registration page
[<CLIMutable; NoComparison; NoEquality>]
type RegisterViewModel =
    {   /// The user's first name
        FirstName : string

        /// The user's last name
        LastName : string

        /// The user's display name
        DisplayName : string option

        /// The user's e-mail address
        Email : string

        /// The user's desired password
        Password : string

        /// The index of the first question asked
        Question1Index : int

        /// The answer for the first question asked
        Question1Answer : string

        /// The index of the second question asked
        Question2Index : int

        /// The answer for the second question asked
        Question2Answer : string
    }

/// Support for the registration page view model
module RegisterViewModel =

    /// An empty view model
    let empty =
        {   FirstName       = ""
            LastName        = ""
            DisplayName     = None
            Email           = ""
            Password        = ""
            Question1Index  = 0
            Question1Answer = ""
            Question2Index  = 0
            Question2Answer = ""
        }
