module JobsJobsJobs.SuccessStories.Data

open JobsJobsJobs.Common.Data
open JobsJobsJobs.Domain
open JobsJobsJobs.SuccessStories.Domain
open Npgsql.FSharp

// Retrieve all success stories  
let all () =
    dataSource ()
    |> Sql.query $"
        SELECT s.*, c.data AS cit_data
          FROM {Table.Success} s
               INNER JOIN {Table.Citizen} c ON c.id = s.data ->> 'citizenId'
         ORDER BY s.data ->> 'recordedOn' DESC"
    |> Sql.executeAsync (fun row ->
        let success = toDocument<Success> row
        let citizen = toDocumentFrom<Citizen> "cit_data" row
        {   Id          = success.Id
            CitizenId   = success.CitizenId
            CitizenName = Citizen.name citizen
            RecordedOn  = success.RecordedOn
            FromHere    = success.IsFromHere
            HasStory    = Option.isSome success.Story
        })

/// Find a success story by its ID
let findById successId =
    dataSource () |> getDocument<Success> Table.Success (SuccessId.toString successId)

/// Save a success story
let save (success : Success) =
    (dataSource (), mkDoc success) ||> saveDocument Table.Success (SuccessId.toString success.Id)
