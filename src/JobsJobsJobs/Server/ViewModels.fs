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
