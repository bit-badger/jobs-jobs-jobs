/// View models for Jobs, Jobs, Jobs
module JobsJobsJobs.ViewModels

open JobsJobsJobs.Domain

/// The fields required for a skill
[<CLIMutable; NoComparison; NoEquality>]
type SkillForm =
    {   /// The ID of this skill
        Id : string
        
        /// The description of the skill
        Description : string
        
        /// Notes regarding the skill
        Notes : string option
    }

/// The data required to update a profile
[<CLIMutable; NoComparison; NoEquality>]
type EditProfileViewModel =
    {   /// Whether the citizen to whom this profile belongs is actively seeking employment
        IsSeekingEmployment : bool
        
        /// Whether this profile should appear in the public search
        IsPublic : bool
        
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
        
        /// The user's past experience
        Experience : string option
        
        /// The skills for the user
        Skills : SkillForm array
    }

/// Support functions for the ProfileForm type
module EditProfileViewModel =

    /// An empty view model (used for new profiles)
    let empty =
        {   IsSeekingEmployment = false
            IsPublic            = false
            ContinentId         = ""
            Region              = ""
            RemoteWork          = false
            FullTime            = false
            Biography           = ""
            Experience          = None
            Skills              = [||]
        }
    
    /// Create an instance of this form from the given profile
    let fromProfile (profile : Profile) =
        {   IsSeekingEmployment = profile.IsSeekingEmployment
            IsPublic            = profile.IsPubliclySearchable
            ContinentId         = string profile.ContinentId
            Region              = profile.Region
            RemoteWork          = profile.IsRemote
            FullTime            = profile.IsFullTime
            Biography           = MarkdownString.toString profile.Biography
            Experience          = profile.Experience |> Option.map MarkdownString.toString
            Skills              = profile.Skills
                                  |> List.map (fun s ->
                                      {   Id          = string s.Id
                                          Description = s.Description
                                          Notes       = s.Notes
                                      })
                                  |> Array.ofList
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
