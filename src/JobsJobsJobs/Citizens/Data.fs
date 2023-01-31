module JobsJobsJobs.Citizens.Data

open JobsJobsJobs.Common.Data
open JobsJobsJobs.Domain
open NodaTime
open Npgsql.FSharp

/// The last time a token purge check was run
let mutable private lastPurge = Instant.MinValue

/// Lock access to the above
let private locker = obj ()

/// Delete a citizen by their ID using the given connection properties
let private doDeleteById citizenId connProps = backgroundTask {
    let! _ =
        connProps
        |> Sql.query $"
            DELETE FROM {Table.Success} WHERE data ->> 'citizenId' = @id;
            DELETE FROM {Table.Listing} WHERE data ->> 'citizenId' = @id;
            DELETE FROM {Table.Citizen} WHERE id                   = @id"
        |> Sql.parameters [ "@id", Sql.string (CitizenId.toString citizenId) ]
        |> Sql.executeNonQueryAsync
    ()
}

/// Delete a citizen by their ID
let deleteById citizenId =
    doDeleteById citizenId (dataSource ())

/// Save a citizen
let private saveCitizen (citizen : Citizen) connProps =
    saveDocument Table.Citizen (CitizenId.toString citizen.Id) connProps (mkDoc citizen) 

/// Save security information for a citizen
let private saveSecurity (security : SecurityInfo) connProps =
    saveDocument Table.SecurityInfo (CitizenId.toString security.Id) connProps (mkDoc security)

/// Purge expired tokens
let private purgeExpiredTokens now = backgroundTask {
    let connProps = dataSource ()
    let! info =
        Sql.query $"SELECT * FROM {Table.SecurityInfo} WHERE data ->> 'tokenExpires' IS NOT NULL" connProps
        |> Sql.executeAsync toDocument<SecurityInfo>
    for expired in info |> List.filter (fun it -> it.TokenExpires.Value < now) do
        if expired.TokenUsage.Value = "confirm" then
            // Unconfirmed account; delete the entire thing
            do! doDeleteById expired.Id connProps
        else
            // Some other use; just clear the token
            do! saveSecurity { expired with Token = None; TokenUsage = None; TokenExpires = None } connProps
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
let findById citizenId = backgroundTask {
    match! dataSource () |> getDocument<Citizen> Table.Citizen (CitizenId.toString citizenId) with
    | Some c when not c.IsLegacy -> return Some c
    | Some _
    | None -> return None
}

/// Save a citizen
let save citizen =
    saveCitizen citizen (dataSource ())
    
/// Register a citizen (saves citizen and security settings); returns false if the e-mail is already taken
let register citizen (security : SecurityInfo) = backgroundTask {
    let  connProps = dataSource ()
    use  conn      = Sql.createConnection connProps
    use! txn       = conn.BeginTransactionAsync ()
    try
        do! saveCitizen  citizen  connProps
        do! saveSecurity security connProps
        do! txn.CommitAsync ()
        return true
    with
    | :? Npgsql.PostgresException as ex when ex.SqlState = "23505" && ex.ConstraintName = "uk_citizen_email" ->
        do! txn.RollbackAsync ()
        return false
}

/// Try to find the security information matching a confirmation token
let private tryConfirmToken token connProps = backgroundTask {
    let! tryInfo =
        connProps
        |> Sql.query $"
            SELECT *
              FROM {Table.SecurityInfo}
             WHERE data ->> 'token'      = @token
               AND data ->> 'tokenUsage' = 'confirm'"
        |> Sql.parameters [ "@token", Sql.string token ]
        |> Sql.executeAsync toDocument<SecurityInfo>
    return List.tryHead tryInfo
}

/// Confirm a citizen's account
let confirmAccount token = backgroundTask {
    do! checkForPurge true
    let connProps = dataSource ()
    match! tryConfirmToken token connProps with
    | Some info ->
        do! saveSecurity { info with AccountLocked = false; Token = None; TokenUsage = None; TokenExpires = None }
                connProps
        return true
    | None -> return false
}
    
/// Deny a citizen's account (user-initiated; used if someone used their e-mail address without their consent)
let denyAccount token = backgroundTask {
    do! checkForPurge true
    let connProps = dataSource ()
    match! tryConfirmToken token connProps with
    | Some info ->
        do! doDeleteById info.Id connProps
        return true
    | None -> return false
}
    
/// Attempt a user log on
let tryLogOn email password (pwVerify : Citizen -> string -> bool option) (pwHash : Citizen -> string -> string)
        now = backgroundTask {
    do! checkForPurge false
    let  connProps  = dataSource ()
    let! tryCitizen =
        connProps
        |> Sql.query $"
            SELECT *
              FROM {Table.Citizen}
             WHERE data ->> 'email'    = @email
               AND data ->> 'isLegacy' = 'false'"
        |> Sql.parameters [ "@email", Sql.string email ]
        |> Sql.executeAsync toDocument<Citizen>
    match List.tryHead tryCitizen with
    | Some citizen ->
        let citizenId = CitizenId.toString citizen.Id
        let! tryInfo = getDocument<SecurityInfo> Table.SecurityInfo citizenId connProps
        let! info = backgroundTask {
            match tryInfo with
            | Some it -> return it
            | None ->
                let it = { SecurityInfo.empty with Id = citizen.Id }
                do! saveSecurity it connProps
                return it
        }
        if info.AccountLocked then return Error "Log on unsuccessful (Account Locked)"
        else
            match pwVerify citizen password with
            | Some rehash ->
                let hash = if rehash then pwHash citizen password else citizen.PasswordHash
                do! saveSecurity { info with FailedLogOnAttempts = 0 } connProps
                do! saveCitizen { citizen with LastSeenOn = now; PasswordHash = hash } connProps
                return Ok { citizen with LastSeenOn = now }
            | None ->
                let locked = info.FailedLogOnAttempts >= 4
                do! { info with FailedLogOnAttempts = info.FailedLogOnAttempts + 1; AccountLocked = locked }
                    |> saveSecurity <| connProps
                return Error $"""Log on unsuccessful{if locked then " - Account is now locked" else ""}"""
    | None -> return Error "Log on unsuccessful"
}

/// Try to retrieve a citizen and their security information by their e-mail address
let tryByEmailWithSecurity email = backgroundTask {
    let toCitizenSecurityPair row = (toDocument<Citizen> row, toDocumentFrom<SecurityInfo> "sec_data" row)
    let! results =
        dataSource ()
        |> Sql.query $"
            SELECT c.*, s.data AS sec_data
              FROM {Table.Citizen} c
                   INNER JOIN {Table.SecurityInfo} s ON s.id = c.id
             WHERE c.data ->> 'email' = @email"
        |> Sql.parameters [ "@email", Sql.string email ]
        |> Sql.executeAsync toCitizenSecurityPair
    return List.tryHead results
}

/// Save an updated security information document
let saveSecurityInfo security = backgroundTask {
    do! saveSecurity security (dataSource ())
}

/// Try to retrieve security information by the given token
let trySecurityByToken token = backgroundTask {
    do! checkForPurge false
    let! results =
        dataSource ()
        |> Sql.query $"SELECT * FROM {Table.SecurityInfo} WHERE data ->> 'token' = @token"
        |> Sql.parameters [ "@token", Sql.string token ]
        |> Sql.executeAsync toDocument<SecurityInfo>
    return List.tryHead results
}

// ~~~ LEGACY MIGRATION ~~~ //

/// Get all legacy citizens
let legacy () = backgroundTask {
    return!
        dataSource ()
        |> Sql.query $"SELECT * FROM {Table.Citizen} WHERE data ->> 'isLegacy' = 'true'"
        |> Sql.executeAsync toDocument<Citizen>
}
