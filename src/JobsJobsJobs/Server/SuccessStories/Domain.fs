module JobsJobsJobs.SuccessStories.Domain

open JobsJobsJobs.Domain
open NodaTime

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


/// An entry in the list of success stories
[<NoComparison; NoEquality>]
type StoryEntry =
    {   /// The ID of this success story
        Id : SuccessId
        
        /// The ID of the citizen who recorded this story
        CitizenId : CitizenId
        
        /// The name of the citizen who recorded this story
        CitizenName : string
        
        /// When this story was recorded
        RecordedOn : Instant
        
        /// Whether this story involves an opportunity that arose due to Jobs, Jobs, Jobs
        FromHere : bool
        
        /// Whether this report has a further story, or if it is simply a "found work" entry
        HasStory : bool
    }
