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
    let citId = CitizenId.toString citizenId
    let! _ =
        connProps
        |> Sql.query $"
            DELETE FROM {Table.Success} WHERE data @> @criteria;
            DELETE FROM {Table.Listing} WHERE data @> @criteria;
            DELETE FROM {Table.Citizen} WHERE id    = @id"
        |> Sql.parameters [ "@criteria", Sql.jsonb (mkDoc {| citizenId = citId |}); "@id", Sql.string citId ]
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
let private tryConfirmToken (token : string) connProps = backgroundTask {
    let! tryInfo =
        connProps
        |> Sql.query $" SELECT * FROM {Table.SecurityInfo} WHERE data @> @criteria"
        |> Sql.parameters [ "criteria", Sql.jsonb (mkDoc {| token = token; tokenUsage = "confirm" |}) ]
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
        |> Sql.query $"SELECT * FROM {Table.Citizen} WHERE data @> @criteria"
        |> Sql.parameters [ "@criteria", Sql.jsonb (mkDoc {| email = email; isLegacy = false |}) ]
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
             WHERE c.data @> @criteria"
        |> Sql.parameters [ "@criteria", Sql.jsonb (mkDoc {| email = email |}) ]
        |> Sql.executeAsync toCitizenSecurityPair
    return List.tryHead results
}

/// Save an updated security information document
let saveSecurityInfo security = backgroundTask {
    do! saveSecurity security (dataSource ())
}

/// Try to retrieve security information by the given token
let trySecurityByToken (token : string) = backgroundTask {
    do! checkForPurge false
    let! results =
        dataSource ()
        |> Sql.query $"SELECT * FROM {Table.SecurityInfo} WHERE data @> @criteria"
        |> Sql.parameters [ "@criteria", Sql.jsonb (mkDoc {| token = token |}) ]
        |> Sql.executeAsync toDocument<SecurityInfo>
    return List.tryHead results
}

// ~~~ LEGACY MIGRATION ~~~ //

/// Get all legacy citizens
let legacy () = backgroundTask {
    return!
        dataSource ()
        |> Sql.query $"""
            SELECT *
              FROM {Table.Citizen}
             WHERE data @> '{{ "isLegacy": true }}'::jsonb
             ORDER BY data ->> 'firstName'"""
        |> Sql.executeAsync toDocument<Citizen>
}

/// Get all current citizens with verified accounts but without a profile
let current () = backgroundTask {
    return!
        dataSource ()
        |> Sql.query $"""
            SELECT c.*
              FROM {Table.Citizen} c
                   INNER JOIN {Table.SecurityInfo} si ON si.id = c.id
             WHERE c.data  @> '{{ "isLegacy": false }}'::jsonb
               AND si.data @> '{{ "accountLocked": false }}'::jsonb
               AND NOT EXISTS (SELECT 1 FROM {Table.Profile} p WHERE p.id = c.id)"""
        |> Sql.executeAsync toDocument<Citizen>
}

let migrateLegacy currentId legacyId = backgroundTask {
    let  oldId     = CitizenId.toString legacyId
    let  connProps = dataSource ()
    use  conn      = Sql.createConnection connProps
    use! txn       = conn.BeginTransactionAsync ()
    try
        // Add legacy data to current user
        let! profiles =
            conn
            |> Sql.existingConnection
            |> Sql.query $"SELECT * FROM {Table.Profile} WHERE id = @oldId"
            |> Sql.parameters [ "@oldId", Sql.string oldId ]
            |> Sql.executeAsync toDocument<Profile>
        match List.tryHead profiles with
        | Some profile ->
            do! saveDocument
                    Table.Profile (CitizenId.toString currentId) (Sql.existingConnection conn)
                    (mkDoc { profile with Id = currentId; IsLegacy = false })
        | None -> ()
        let  oldCriteria = mkDoc {| citizenId = oldId |}
        let! listings    =
            conn
            |> Sql.existingConnection
            |> Sql.query $"SELECT * FROM {Table.Listing} WHERE data @> @criteria"
            |> Sql.parameters [ "@criteria", Sql.jsonb oldCriteria ]
            |> Sql.executeAsync toDocument<Listing>
        for listing in listings do
            let newListing = { listing with Id = ListingId.create (); CitizenId = currentId; IsLegacy = false }
            do! saveDocument
                    Table.Listing (ListingId.toString newListing.Id) (Sql.existingConnection conn) (mkDoc newListing)
        let! successes =
            conn
            |> Sql.existingConnection
            |> Sql.query $"SELECT * FROM {Table.Success} WHERE data @> @criteria"
            |> Sql.parameters [ "@criteria", Sql.string oldCriteria ]
            |> Sql.executeAsync toDocument<Success>
        for success in successes do
            let newSuccess = { success with Id = SuccessId.create (); CitizenId = currentId }
            do! saveDocument
                    Table.Success (SuccessId.toString newSuccess.Id) (Sql.existingConnection conn) (mkDoc newSuccess)
        // Delete legacy data
        let! _ =
            conn
            |> Sql.existingConnection
            |> Sql.query $"
                DELETE FROM {Table.Success} WHERE data @> @criteria;
                DELETE FROM {Table.Listing} WHERE data @> @criteria;
                DELETE FROM {Table.Citizen} WHERE id    = @oldId"
            |> Sql.parameters [ "@criteria", Sql.jsonb oldCriteria; "@oldId", Sql.string oldId ]
            |> Sql.executeNonQueryAsync
        do! txn.CommitAsync ()
        return Ok ""
    with :? Npgsql.PostgresException as ex ->
        do! txn.RollbackAsync ()
        return Error ex.MessageText
}
