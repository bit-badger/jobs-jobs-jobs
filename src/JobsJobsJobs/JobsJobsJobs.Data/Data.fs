namespace JobsJobsJobs.Data

open JobsJobsJobs.Domain
open Marten
open Marten.PLv8
open Microsoft.Extensions.Configuration

/// Connection management for the Marten document store
module Connection =
    
    open Weasel.Core
    
    /// The configuration from which a document store will be created
    let mutable private config : IConfiguration option = None
    
    /// Lazy initialization for the Marten document store, constructed when setUp() is called
    let private lazyStore = lazy (task {
        match config with
        | Some cfg ->
            let store =
                DocumentStore.For(fun opts ->
                    opts.Connection (cfg.GetConnectionString "PostgreSQL")
                    opts.RegisterDocumentTypes [
                        typeof<Citizen>; typeof<Continent>; typeof<Listing>; typeof<Profile>; typeof<SecurityInfo>
                        typeof<Success>
                    ]
                    opts.AutoCreateSchemaObjects <- AutoCreate.CreateOrUpdate
                    opts.UseJavascriptTransformsAndPatching ()
                    
                    let _ = opts.Schema.For<Citizen>().Identity (fun c -> c.DbId)
                    let _ = opts.Schema.For<SecurityInfo>().Identity (fun si -> si.DbId)
                    ())
            do! store.Storage.ApplyAllConfiguredChangesToDatabaseAsync ()
            return Ok store
        | None -> return Error "Connection.setUp() must be called before accessing a document session"
    })

    /// Set up the data connection from the given configuration
    let setUp (cfg : IConfiguration) =
        config <- Some cfg
        ignore (lazyStore.Force ())
    
    /// A read-only document session
    let querySession () =
        match lazyStore.Force().Result with
        | Ok store -> store.QuerySession ()
        | Error msg -> raise (invalidOp msg)
    
    /// A read/write document session
    let docSession () =
        match lazyStore.Force().Result with
        | Ok store -> store.LightweightSession ()
        | Error msg -> raise (invalidOp msg)


/// Helper functions for data retrieval
[<AutoOpen>]
module private Helpers =
    
    open System.Threading
    
    /// Convert a possibly-null record type to an option
    let optional<'T> (value : 'T) = if isNull (box value) then None else Some value
    
    /// Shorthand for no cancellation token
    let noCnx = CancellationToken.None


open System.Linq
open Connection
open Marten.PLv8.Patching

/// Citizen data access functions
[<RequireQualifiedAccess>]
module Citizens =

    /// Delete a citizen by their ID
    let deleteById citizenId = backgroundTask {
        use session = docSession ()
        session.Delete<Citizen> (CitizenId.value citizenId)
        do! session.SaveChangesAsync ()
    }
    
    /// Find a citizen by their ID
    let findById citizenId = backgroundTask {
        use session = querySession ()
        let! citizen = session.LoadAsync<Citizen> (CitizenId.value citizenId)
        return
            match optional citizen with
            | Some c when not c.isLegacy -> Some c
            | Some _
            | None -> None
    }
    
    /// Save a citizen
    let save (citizen : Citizen) = backgroundTask {
        use session = docSession ()
        session.Store<Citizen> citizen
        do! session.SaveChangesAsync ()
    }
    
    /// Attempt a user log on
    let tryLogOn email (pwCheck : string -> bool) now = backgroundTask {
        use session = docSession ()
        let! tryCitizen =
            session.Query<Citizen>().Where(fun c -> c.email = email && not c.isLegacy).SingleOrDefaultAsync ()
        match optional tryCitizen with
        | Some citizen ->
            let! tryInfo = session.LoadAsync<SecurityInfo> citizen.DbId
            let! info = backgroundTask {
                match optional tryInfo with
                | Some it -> return it
                | None ->
                    let it = { SecurityInfo.empty with Id = citizen.id }
                    session.Store<SecurityInfo> it
                    do! session.SaveChangesAsync ()
                    return it
            }
            if info.AccountLocked then return Error "Log on unsuccessful (Account Locked)"
            elif pwCheck citizen.passwordHash then
                session.Patch<SecurityInfo>(citizen.DbId).Set((fun si -> si.FailedLogOnAttempts), 0)
                session.Patch<Citizen>(citizen.DbId).Set((fun c -> c.lastSeenOn), now)
                do! session.SaveChangesAsync ()
                return Ok { citizen with lastSeenOn = now }
            else
                let locked = info.FailedLogOnAttempts >= 4
                session.Patch<SecurityInfo>(citizen.DbId).Increment(fun si -> si.FailedLogOnAttempts)
                if locked then session.Patch<SecurityInfo>(citizen.DbId).Set((fun si -> si.AccountLocked), true)
                do! session.SaveChangesAsync ()
                return Error $"""Log on unsuccessful{if locked then " - Account is now locked" else ""}"""
        | None -> return Error "Log on unsuccessful"
    }


/// Continent data access functions
[<RequireQualifiedAccess>]
module Continents =
    
    /// Retrieve all continents
    let all () = backgroundTask {
        use session = querySession ()
        let! it = session.Query<Continent>().ToListAsync<Continent> noCnx
        return List.ofSeq it
    }
    
    /// Retrieve a continent by its ID
    let findById continentId = backgroundTask {
        use session = querySession ()
        let! tryContinent = session.LoadAsync<Continent> (ContinentId.value continentId)
        return optional tryContinent
    }


open System
open JobsJobsJobs.Domain.SharedTypes

/// Job listing access functions
[<RequireQualifiedAccess>]
module Listings =
    
    open System.Collections.Generic
    
    /// Find all job listings posted by the given citizen
    let findByCitizen citizenId = backgroundTask {
        use  session    = querySession ()
        let  continents = Dictionary<ContinentId, Continent> ()
        let! listings   =
            session.Query<Listing>()
                .Include((fun l -> l.continentId :> obj), continents)
                .Where(fun l -> l.citizenId = citizenId && not l.isLegacy)
                .ToListAsync ()
        return
            listings
            |> Seq.map (fun l -> { listing = l; continent = continents[l.continentId] })
            |> List.ofSeq
    }
    
    /// Find a listing by its ID
    let findById listingId = backgroundTask {
        use  session    = querySession ()
        let! tryListing = session.LoadAsync<Listing> (ListingId.value listingId)
        match optional tryListing with
        | Some listing when not listing.isLegacy -> return Some listing
        | Some _
        | None -> return None
    }
    
    /// Find a listing by its ID for viewing (includes continent information)
    let findByIdForView listingId = backgroundTask {
        use session = querySession ()
        let mutable continent = Continent.empty
        let! tryListing =
            session.Query<Listing>()
                .Include((fun l -> l.continentId :> obj), fun c -> continent <- c)
                .Where(fun l -> l.id = listingId && not l.isLegacy)
                .SingleOrDefaultAsync ()
        match optional tryListing with
        | Some listing -> return Some { listing = listing; continent = continent }
        | None -> return None
    }
    
    /// Save a listing
    let save (listing : Listing) = backgroundTask {
        use session = docSession ()
        session.Store listing
        do! session.SaveChangesAsync ()
    }
    
    /// Search job listings
    let search (search : ListingSearch) = backgroundTask {
        use session     = querySession ()
        let continents  = Dictionary<ContinentId, Continent> ()
        let searchQuery =
            seq<Listing -> bool> {
                match search.continentId with
                | Some contId ->
                    fun (l : Listing) -> l.continentId = (ContinentId.ofString contId)
                | None -> ()
                match search.region with
                | Some region -> fun (l : Listing) -> l.region.Contains (region, StringComparison.OrdinalIgnoreCase)
                | None -> ()
                if search.remoteWork <> "" then
                    fun (l : Listing) -> l.remoteWork = (search.remoteWork = "yes")
                // match search.text with
                // | Some text -> fun (l : Listing) -> l.text.Contains (text, StringComparison.OrdinalIgnoreCase)
                // | None -> ()
            }
            |> Seq.fold
                   (fun q filter -> Queryable.Where(q, filter))
                   (session.Query<Listing>()
                        .Include((fun l -> l.continentId :> obj), continents)
                        .Where(fun l -> not l.isExpired && not l.isLegacy))
        let! results = searchQuery.ToListAsync ()
        return
            results
            |> Seq.map (fun l -> { listing = l; continent = continents[l.continentId] })
            |> List.ofSeq
    }

/// Success story data access functions
[<RequireQualifiedAccess>]
module Successes =
    
    /// Save a success story
    let save (success : Success) = backgroundTask {
        use session = docSession ()
        session.Store<Success> success
        do! session.SaveChangesAsync ()
    }
    