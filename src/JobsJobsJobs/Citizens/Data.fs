module JobsJobsJobs.Citizens.Data

open BitBadger.Npgsql.FSharp.Documents
open JobsJobsJobs.Common.Data
open JobsJobsJobs.Domain
open NodaTime
open Npgsql.FSharp

/// The last time a token purge check was run
let mutable private lastPurge = Instant.MinValue

/// Lock access to the above
let private locker = obj ()

/// Delete a citizen by their ID
let deleteById citizenId = backgroundTask {
    let citId = CitizenId.toString citizenId
    do! Custom.nonQuery
            $"{Query.Delete.byContains Table.Success};
              {Query.Delete.byContains Table.Listing};
              {Query.Delete.byId       Table.Citizen}"
            [ "@criteria", Query.jsonbDocParam {| citizenId = citId |}; "@id", Sql.string citId ]
}

/// Save a citizen
let private saveCitizen (citizen : Citizen) =
    save Table.Citizen (CitizenId.toString citizen.Id) citizen

/// Save security information for a citizen
let saveSecurityInfo (security : SecurityInfo) =
    save Table.SecurityInfo (CitizenId.toString security.Id) security

/// Purge expired tokens
let private purgeExpiredTokens now = backgroundTask {
    let! info =
        Custom.list $"{Query.selectFromTable Table.SecurityInfo} WHERE data ->> 'tokenExpires' IS NOT NULL" []
                    fromData<SecurityInfo>
    for expired in info |> List.filter (fun it -> it.TokenExpires.Value < now) do
        if expired.TokenUsage.Value = "confirm" then
            // Unconfirmed account; delete the entire thing
            do! deleteById expired.Id
        else
            // Some other use; just clear the token
            do! saveSecurityInfo { expired with Token = None; TokenUsage = None; TokenExpires = None }
}

/// Check for tokens to purge if it's been more than 10 minutes since we last checked
let private checkForPurge skipCheck =
    lock locker (fun () -> backgroundTask {
        let now  = SystemClock.Instance.GetCurrentInstant ()
        if skipCheck || (now - lastPurge).TotalMinutes >= 10 then
            do! purgeExpiredTokens now
            lastPurge <- now
    })

/// Find a citizen by their ID
let findById citizenId =
    Find.byId Table.Citizen (CitizenId.toString citizenId)

/// Save a citizen
let save citizen =
    saveCitizen citizen
    
/// Register a citizen (saves citizen and security settings); returns false if the e-mail is already taken
let register (citizen : Citizen) (security : SecurityInfo) = backgroundTask {
    try
        let! _ =
            Configuration.dataSource ()
            |> Sql.fromDataSource
            |> Sql.executeTransactionAsync
                [   Query.save Table.Citizen,      [ Query.docParameters (CitizenId.toString citizen.Id) citizen  ]
                    Query.save Table.SecurityInfo, [ Query.docParameters (CitizenId.toString citizen.Id) security ]
                ]
        return true
    with
    | :? Npgsql.PostgresException as ex when ex.SqlState = "23505" && ex.ConstraintName = "uk_citizen_email" ->
        return false
}

/// Try to find the security information matching a confirmation token
let private tryConfirmToken (token : string) = backgroundTask {
    let! tryInfo = Find.byContains<SecurityInfo> Table.SecurityInfo {| token = token; tokenUsage = "confirm" |}
    return List.tryHead tryInfo
}

/// Confirm a citizen's account
let confirmAccount token = backgroundTask {
    do! checkForPurge true
    match! tryConfirmToken token with
    | Some info ->
        do! saveSecurityInfo { info with AccountLocked = false; Token = None; TokenUsage = None; TokenExpires = None }
        return true
    | None -> return false
}
    
/// Deny a citizen's account (user-initiated; used if someone used their e-mail address without their consent)
let denyAccount token = backgroundTask {
    do! checkForPurge true
    match! tryConfirmToken token with
    | Some info ->
        do! deleteById info.Id
        return true
    | None -> return false
}
    
/// Attempt a user log on
let tryLogOn email password (pwVerify : Citizen -> string -> bool option) (pwHash : Citizen -> string -> string)
        now = backgroundTask {
    do! checkForPurge false
    let! tryCitizen = Find.byContains<Citizen> Table.Citizen {| email = email |}
    match List.tryHead tryCitizen with
    | Some citizen ->
        let citizenId = CitizenId.toString citizen.Id
        let! tryInfo = Find.byId<SecurityInfo> Table.SecurityInfo citizenId
        let! info = backgroundTask {
            match tryInfo with
            | Some it -> return it
            | None ->
                let it = { SecurityInfo.empty with Id = citizen.Id }
                do! saveSecurityInfo it
                return it
        }
        if info.AccountLocked then return Error "Log on unsuccessful (Account Locked)"
        else
            match pwVerify citizen password with
            | Some rehash ->
                let hash = if rehash then pwHash citizen password else citizen.PasswordHash
                do! saveSecurityInfo { info with FailedLogOnAttempts = 0 }
                do! saveCitizen { citizen with LastSeenOn = now; PasswordHash = hash }
                return Ok { citizen with LastSeenOn = now }
            | None ->
                let locked = info.FailedLogOnAttempts >= 4
                do! { info with FailedLogOnAttempts = info.FailedLogOnAttempts + 1; AccountLocked = locked }
                    |> saveSecurityInfo
                return Error $"""Log on unsuccessful{if locked then " - Account is now locked" else ""}"""
    | None -> return Error "Log on unsuccessful"
}

/// Try to retrieve a citizen and their security information by their e-mail address
let tryByEmailWithSecurity email =
    Custom.single
        $"SELECT c.*, s.data AS sec_data
            FROM {Table.Citizen} c
                 INNER JOIN {Table.SecurityInfo} s ON s.id = c.id
           WHERE c.data @> @criteria"
        [ "@criteria", Query.jsonbDocParam {| email = email |} ]
        (fun row -> (fromData<Citizen> row, fromDocument<SecurityInfo> "sec_data" row))

/// Try to retrieve security information by the given token
let trySecurityByToken (token : string) = backgroundTask {
    do! checkForPurge false
    let! results = Find.byContains<SecurityInfo> Table.SecurityInfo {| token = token |}
    return List.tryHead results
}
