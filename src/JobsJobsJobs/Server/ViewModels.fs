/// View models for Jobs, Jobs, Jobs
module JobsJobsJobs.ViewModels

/// View model for the log on page
type LogOnViewModel =
    {   /// A message regarding an error encountered during a log on attempt
        ErrorMessage : string option

        /// The e-mail address for the user attempting to log on
        Email : string

        /// The password of the user attempting to log on
        Password : string
    }


/// View model for the registration page
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
