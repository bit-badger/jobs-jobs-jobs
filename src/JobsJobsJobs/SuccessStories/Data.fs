module JobsJobsJobs.SuccessStories.Data

open BitBadger.Npgsql.FSharp.Documents
open JobsJobsJobs.Common.Data
open JobsJobsJobs.Domain
open JobsJobsJobs.SuccessStories.Domain

// Retrieve all success stories  
let all () =
    Custom.list<StoryEntry>
        $" SELECT s.*, c.data AS cit_data
             FROM {Table.Success} s
                  INNER JOIN {Table.Citizen} c ON c.id = s.data ->> 'citizenId'
            ORDER BY s.data ->> 'recordedOn' DESC"
        []
        (fun row ->
            let success = fromData<Success> row
            let citizen = fromDocument<Citizen> "cit_data" row
            {   Id          = success.Id
                CitizenId   = success.CitizenId
                CitizenName = Citizen.name citizen
                RecordedOn  = success.RecordedOn
                FromHere    = success.IsFromHere
                HasStory    = Option.isSome success.Story
            })

/// Find a success story by its ID
let findById successId =
    Find.byId<Success> Table.Success (SuccessId.toString successId)

/// Save a success story
let save (success : Success) =
    save Table.Success (SuccessId.toString success.Id) success
