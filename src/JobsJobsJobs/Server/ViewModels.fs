/// View models for Jobs, Jobs, Jobs
module JobsJobsJobs.ViewModels

open JobsJobsJobs.Domain

/// The data to add or update an other contact
[<CLIMutable; NoComparison; NoEquality>]
type OtherContactForm =
    {   /// The type of the contact
        ContactType : string
        
        /// The name of the contact
        Name : string
        
        /// The value of the contact (URL, e-mail address, phone, etc.)
        Value : string
        
        /// Whether this contact is displayed for public employment profiles and job listings
        IsPublic : bool
    }

/// Support functions for the contact form
module OtherContactForm =

    /// Create a contact form from a contact
    let fromContact (contact : OtherContact) =
        {   ContactType = ContactType.toString contact.ContactType
            Name        = defaultArg contact.Name ""
            Value       = contact.Value
            IsPublic    = contact.IsPublic
        }


/// The data available to update an account profile
[<CLIMutable; NoComparison; NoEquality>]
type AccountProfileForm =
    {   /// The first name of the citizen
        FirstName : string
        
        /// The last name of the citizen
        LastName : string
        
        /// The display name for the citizen
        DisplayName : string
        
        /// The citizen's new password
        NewPassword : string
        
        /// Confirmation of the citizen's new password
        NewPasswordConfirm : string
        
        /// The contacts for this profile
        Contacts : OtherContactForm array
    }

/// Support functions for the account profile form
module AccountProfileForm =

    /// Create an account profile form from a citizen
    let fromCitizen (citizen : Citizen) =
        {   FirstName          = citizen.FirstName
            LastName           = citizen.LastName
            DisplayName        = defaultArg citizen.DisplayName ""
            NewPassword        = ""
            NewPasswordConfirm = ""
            Contacts           = citizen.OtherContacts |> List.map OtherContactForm.fromContact |> Array.ofList
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


/// The data required to provide a success story
[<CLIMutable; NoComparison; NoEquality>]
type EditSuccessForm =
    {   /// The ID of this success story
        Id : string
        
        /// Whether the employment was obtained from Jobs, Jobs, Jobs
        FromHere : bool
        
        /// The success story
        Story : string
    }

/// Support functions for success edit forms
module EditSuccessForm =

    /// Create an edit form from a success story
    let fromSuccess (success : Success) =
        {   Id       = SuccessId.toString success.Id
            FromHere = success.IsFromHere
            Story    = success.Story |> Option.map MarkdownString.toString |> Option.defaultValue ""
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
