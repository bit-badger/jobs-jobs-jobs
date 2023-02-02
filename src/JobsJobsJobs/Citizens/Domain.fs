module JobsJobsJobs.Citizens.Domain

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


/// Form for the forgot / reset password page
[<CLIMutable; NoComparison; NoEquality>]
type ForgotPasswordForm =
    {   /// The e-mail address for the account wishing to reset their password
        Email : string
    }


/// Form for the log on page
[<CLIMutable; NoComparison; NoEquality>]
type LogOnForm =
    {   /// A message regarding an error encountered during a log on attempt
        ErrorMessage : string option

        /// The e-mail address for the user attempting to log on
        Email : string

        /// The password of the user attempting to log on
        Password : string

        /// The URL where the user should be redirected after logging on
        ReturnTo : string option
    }


/// Form for the registration page
[<CLIMutable; NoComparison; NoEquality>]
type RegisterForm =
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
module RegisterForm =

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


/// The form for a user resetting their password
[<CLIMutable; NoComparison; NoEquality>]
type ResetPasswordForm =
    {   /// The ID of the citizen whose password is being reset
        Id : string

        /// The verification token for the password reset
        Token : string

        /// The new password for the account
        Password : string
    }

// ~~~ LEGACY MIGRATION ~~ //

[<CLIMutable; NoComparison; NoEquality>]
type LegacyMigrationForm =
    {   /// The ID of the current citizen
        Id : string

        /// The ID of the legacy citizen to be migrated
        LegacyId : string
    }
