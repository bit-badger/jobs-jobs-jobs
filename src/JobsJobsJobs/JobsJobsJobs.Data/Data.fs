namespace JobsJobsJobs.Data

open System
open JobsJobsJobs.Domain

/// Wrapper documents for our record types
module Documents =
    
    /// A generic type that keeps its ID in sync with the ID value for its content
    [<AllowNullLiteral>]
    type Document<'T> (initialValue : 'T, toId : 'T -> Guid) =
        
        /// The current value for this document
        let mutable value = initialValue
        
        /// The ID for this document
        member val Id = toId initialValue with get, set
        
        /// The value for this document
        member this.Value
          with get () = value
           and set (v : 'T) =
               value   <- v
               this.Id <- toId v
        
        /// Convert a document to its value
        static member ToValue (doc : Document<'T>) =
            doc.Value
        
        /// Convert a document to its value, or None if the document is null
        static member TryValue (doc : Document<'T>) =
            if isNull doc then None else Some doc.Value
    
    /// A citizen document
    [<AllowNullLiteral>]
    type CitizenDocument (citizen : Citizen) =
        inherit Document<Citizen> (citizen, fun c -> CitizenId.value c.id)
        new() = CitizenDocument Citizen.empty
    
    /// A continent document
    [<AllowNullLiteral>]
    type ContinentDocument (continent : Continent) =
        inherit Document<Continent> (continent, fun c -> ContinentId.value c.id)
        new () = ContinentDocument Continent.empty
    
    /// A job listing document
    [<AllowNullLiteral>]
    type ListingDocument (listing : Listing) =
        inherit Document<Listing> (listing, fun l -> ListingId.value l.id)
        new () = ListingDocument Listing.empty
    
    /// A profile document
    [<AllowNullLiteral>]
    type ProfileDocument (profile : Profile) =
        inherit Document<Profile> (profile, fun p -> CitizenId.value p.id)
        new () = ProfileDocument Profile.empty
    
    /// A security information document
    [<AllowNullLiteral>]
    type SecurityInfoDocument (securityInfo : SecurityInfo) =
        inherit Document<SecurityInfo> (securityInfo, fun si -> CitizenId.value si.Id)
        new () = SecurityInfoDocument SecurityInfo.empty
    
    /// A success story document
    [<AllowNullLiteral>]
    type SuccessDocument (success : Success) =
        inherit Document<Success> (success, fun s -> SuccessId.value s.id)
        new () = SuccessDocument Success.empty


open Documents
open Marten

/// Connection management for the Marten document store
module Connection =
    
    open Marten.NodaTime
    open Microsoft.Extensions.Configuration
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
                        typeof<CitizenDocument>; typeof<ContinentDocument>;    typeof<ListingDocument>
                        typeof<ProfileDocument>; typeof<SecurityInfoDocument>; typeof<SuccessDocument>
                    ]
                    opts.DatabaseSchemaName      <- "jjj"
                    opts.AutoCreateSchemaObjects <- AutoCreate.CreateOrUpdate
                    opts.UseNodaTime ()
                    
                    let _ = opts.Schema.For<CitizenDocument>().DocumentAlias      "citizen"
                    let _ = opts.Schema.For<ContinentDocument>().DocumentAlias    "continent"
                    let _ = opts.Schema.For<ListingDocument>().DocumentAlias      "listing"
                    let _ = opts.Schema.For<ProfileDocument>().DocumentAlias      "profile"
                    let _ = opts.Schema.For<SecurityInfoDocument>().DocumentAlias "security_info"
                    let _ = opts.Schema.For<SuccessDocument>().DocumentAlias      "success"
                    ())
            do! store.Storage.ApplyAllConfiguredChangesToDatabaseAsync ()
            return Ok store
        | None -> return Error "Connection.setUp() must be called before accessing a document session"
    })

    /// Set up the data connection from the given configuration
    let setUp (cfg : IConfiguration) =
        config <- Some cfg
        lazyStore.Force ()
    
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


/// Shorthand for the generic dictionary
type Dict<'TKey, 'TValue> = System.Collections.Generic.Dictionary<'TKey, 'TValue>


open System.Linq
open Connection

/// Citizen data access functions
[<RequireQualifiedAccess>]
module Citizens =

    /// Delete a citizen by their ID
    let deleteById citizenId = backgroundTask {
        use session = docSession ()
        session.DeleteWhere<SuccessDocument>(fun s -> s.Value.citizenId = citizenId)
        session.DeleteWhere<ListingDocument>(fun l -> l.Value.citizenId = citizenId)
        let docId = CitizenId.value citizenId
        session.Delete<ProfileDocument>      docId
        session.Delete<SecurityInfoDocument> docId
        session.Delete<CitizenDocument>      docId
        do! session.SaveChangesAsync ()
    }
    
    /// Find a citizen by their ID
    let findById citizenId = backgroundTask {
        use session = querySession ()
        let! citizen = session.LoadAsync<CitizenDocument> (CitizenId.value citizenId)
        return
            match Document.TryValue citizen with
            | Some c when not c.isLegacy -> Some c
            | Some _
            | None -> None
    }
    
    /// Save a citizen
    let save (citizen : Citizen) = backgroundTask {
        use session = docSession ()
        session.Store (CitizenDocument citizen)
        do! session.SaveChangesAsync ()
    }
    
    /// Attempt a user log on
    let tryLogOn email (pwCheck : string -> bool) now = backgroundTask {
        use session = docSession ()
        let! tryCitizen =
            session.Query<CitizenDocument>()
                .Where(fun c -> c.Value.email = email && not c.Value.isLegacy)
                .SingleOrDefaultAsync ()
        match Document.TryValue tryCitizen with
        | Some citizen ->
            let! tryInfo = session.LoadAsync<SecurityInfoDocument> (CitizenId.value citizen.id)
            let! info = backgroundTask {
                match Document.TryValue tryInfo with
                | Some it -> return it
                | None ->
                    let it = { SecurityInfo.empty with Id = citizen.id }
                    session.Store (SecurityInfoDocument it)
                    do! session.SaveChangesAsync ()
                    return it
            }
            if info.AccountLocked then return Error "Log on unsuccessful (Account Locked)"
            elif pwCheck citizen.passwordHash then
                session.Store (SecurityInfoDocument { info with FailedLogOnAttempts = 0})
                session.Store (CitizenDocument { citizen with lastSeenOn = now})
                do! session.SaveChangesAsync ()
                return Ok { citizen with lastSeenOn = now }
            else
                let locked = info.FailedLogOnAttempts >= 4
                session.Store (SecurityInfoDocument {
                    info with
                        FailedLogOnAttempts = info.FailedLogOnAttempts + 1
                        AccountLocked       = locked
                })
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
        let! it = session.Query<ContinentDocument>().AsQueryable().ToListAsync ()
        return it |> Seq.map Document.ToValue |> List.ofSeq
    }
    
    /// Retrieve a continent by its ID
    let findById continentId = backgroundTask {
        use session = querySession ()
        let! tryContinent = session.LoadAsync<ContinentDocument> (ContinentId.value continentId)
        return Document.TryValue tryContinent
    }


open JobsJobsJobs.Domain.SharedTypes

/// Job listing access functions
[<RequireQualifiedAccess>]
module Listings =
    
    /// Find all job listings posted by the given citizen
    let findByCitizen citizenId = backgroundTask {
        use  session    = querySession ()
        let  continents = Dict<Guid, ContinentDocument> ()
        let! listings   =
            session.Query<ListingDocument>()
                .Include((fun l -> l.Value.continentId :> obj), continents)
                .Where(fun l -> l.Value.citizenId = citizenId && not l.Value.isLegacy)
                .ToListAsync ()
        return
            listings
            |> Seq.map (fun l -> {
                listing   = l.Value
                continent = continents[ContinentId.value l.Value.continentId].Value
            })
            |> List.ofSeq
    }
    
    /// Find a listing by its ID
    let findById listingId = backgroundTask {
        use  session    = querySession ()
        let! tryListing = session.LoadAsync<ListingDocument> (ListingId.value listingId)
        match Document.TryValue tryListing with
        | Some listing when not listing.isLegacy -> return Some listing
        | Some _
        | None -> return None
    }
    
    /// Find a listing by its ID for viewing (includes continent information)
    let findByIdForView listingId = backgroundTask {
        use session = querySession ()
        let mutable continent : ContinentDocument = null
        let! tryListing =
            session.Query<ListingDocument>()
                .Include((fun l -> l.Value.continentId :> obj), fun c -> continent <- c)
                .Where(fun l -> l.Id = ListingId.value listingId && not l.Value.isLegacy)
                .SingleOrDefaultAsync ()
        match Document.TryValue tryListing with
        | Some listing when not (isNull continent) -> return Some { listing = listing; continent = continent.Value }
        | Some _
        | None -> return None
    }
    
    /// Save a listing
    let save (listing : Listing) = backgroundTask {
        use session = docSession ()
        session.Store (ListingDocument listing)
        do! session.SaveChangesAsync ()
    }
    
    /// Search job listings
    let search (search : ListingSearch) = backgroundTask {
        use session     = querySession ()
        let continents  = Dict<Guid, ContinentDocument> ()
        let searchQuery =
            seq<ListingDocument -> bool> {
                match search.continentId with
                | Some contId ->
                    fun (l : ListingDocument) -> l.Value.continentId = (ContinentId.ofString contId)
                | None -> ()
                match search.region with
                | Some region ->
                    fun (l : ListingDocument) -> l.Value.region.Contains (region, StringComparison.OrdinalIgnoreCase)
                | None -> ()
                if search.remoteWork <> "" then
                    fun (l : ListingDocument) -> l.Value.remoteWork = (search.remoteWork = "yes")
                // match search.text with
                // | Some text -> fun (l : Listing) -> l.text.Contains (text, StringComparison.OrdinalIgnoreCase)
                // | None -> ()
            }
            |> Seq.fold
                   (fun q filter -> Queryable.Where(q, filter))
                   (session.Query<ListingDocument>()
                        .Include((fun l -> l.Value.continentId :> obj), continents)
                        .Where(fun l -> not l.Value.isExpired && not l.Value.isLegacy))
        let! results = searchQuery.ToListAsync ()
        return
            results
            |> Seq.map (fun l -> {
                listing   = l.Value
                continent = continents[ContinentId.value l.Value.continentId].Value
            })
            |> List.ofSeq
    }


/// Profile data access functions
[<RequireQualifiedAccess>]
module Profiles =
    
    /// Count the current profiles
    let count () =
        use session = querySession ()
        session.Query<ProfileDocument>().Where(fun p -> not p.Value.isLegacy).LongCountAsync ()
    
    /// Delete a profile by its ID
    let deleteById citizenId = backgroundTask {
        use session = docSession ()
        session.Delete<ProfileDocument> (CitizenId.value citizenId)
        do! session.SaveChangesAsync ()
    }
    /// Find a profile by citizen ID
    let findById citizenId = backgroundTask {
        use  session = querySession ()
        let! profile = session.LoadAsync<ProfileDocument> (CitizenId.value citizenId)
        return
            match Document.TryValue profile with
            | Some p when not p.isLegacy -> Some p
            | Some _
            | None -> None
    }
    
    /// Find a profile by citizen ID for viewing (includes citizen and continent information)
    let findByIdForView citizenId = backgroundTask {
        use session = querySession ()
        let mutable citizen   : CitizenDocument   = null
        let mutable continent : ContinentDocument = null
        let! tryProfile =
            session.Query<ProfileDocument>()
                .Include<CitizenDocument>((fun p -> p.Id :> obj), fun c -> citizen <- c)
                .Include<ContinentDocument>((fun p -> p.Value.continentId :> obj), fun c -> continent <- c)
                .Where(fun p -> p.Id = CitizenId.value citizenId && not p.Value.isLegacy)
                .SingleOrDefaultAsync ()
        match Document.TryValue tryProfile with
        | Some profile when not (isNull citizen) && not (isNull continent) ->
            return Some { profile = profile; citizen = citizen.Value; continent = continent.Value }
        | Some _
        | None -> return None
    }
    
    /// Save a profile
    let save (profile : Profile) = backgroundTask {
        use session = docSession ()
        session.Store (ProfileDocument profile)
        do! session.SaveChangesAsync ()
    }
    
    /// Search profiles (logged-on users)
    let search (search : ProfileSearch) = backgroundTask {
        use session     = querySession ()
        let citizens    = Dict<Guid, CitizenDocument> ()
        let searchQuery =
            seq<ProfileDocument -> bool> {
                match search.continentId with
                | Some contId -> fun (p : ProfileDocument) -> p.Value.continentId = ContinentId.ofString contId
                | None -> ()
                if search.remoteWork <> "" then
                    fun (p : ProfileDocument) -> p.Value.remoteWork = (search.remoteWork = "yes")
                match search.skill with
                | Some skl ->
                    fun (p : ProfileDocument) ->
                        p.Value.skills.Any(fun s -> s.description.Contains (skl, StringComparison.OrdinalIgnoreCase))
                | None -> ()
                // match search.bioExperience with
                // | Some text ->
                //     let txt = regexContains text
                //     yield filterFunc (fun it -> it.G("biography").Match(txt).Or (it.G("experience").Match txt))
                // | None -> ()
            }
            |> Seq.fold
                (fun q filter -> Queryable.Where(q, filter))
                (session.Query<ProfileDocument>()
                    .Include((fun p -> p.Id :> obj), citizens)
                    .Where(fun p -> not p.Value.isLegacy))
        let! results = searchQuery.ToListAsync ()
        return
            results
            |> Seq.map (fun profileDoc ->
                let p = profileDoc.Value
                {   citizenId         = p.id
                    displayName       = Citizen.name citizens[CitizenId.value p.id].Value
                    seekingEmployment = p.seekingEmployment
                    remoteWork        = p.remoteWork
                    fullTime          = p.fullTime
                    lastUpdatedOn     = p.lastUpdatedOn
                })
            |> Seq.sortBy (fun psr -> psr.displayName.ToLowerInvariant ())
            |> List.ofSeq
    }

    // Search profiles (public)
    let publicSearch (search : PublicSearch) = backgroundTask {
        use session     = querySession ()
        let continents  = Dict<Guid, ContinentDocument> ()
        let searchQuery =
            seq<ProfileDocument -> bool> {
                match search.continentId with
                | Some contId -> fun (p : ProfileDocument) -> p.Value.continentId = ContinentId.ofString contId
                | None -> ()
                match search.region with
                | Some region ->
                    fun (p : ProfileDocument) -> p.Value.region.Contains (region, StringComparison.OrdinalIgnoreCase)
                | None -> ()
                if search.remoteWork <> "" then
                    fun (p : ProfileDocument) -> p.Value.remoteWork = (search.remoteWork = "yes")
                match search.skill with
                | Some skl ->
                    fun (p : ProfileDocument) ->
                        p.Value.skills.Any(fun s -> s.description.Contains (skl, StringComparison.OrdinalIgnoreCase))
                | None -> ()
            }
            |> Seq.fold
                (fun q filter -> Queryable.Where(q, filter))
                (session.Query<ProfileDocument>()
                    .Include((fun p -> p.Value.continentId :> obj), continents)
                    .Where(fun p -> p.Value.isPublic && not p.Value.isLegacy))
        let! results = searchQuery.ToListAsync ()
        return
            results
            |> Seq.map (fun profileDoc ->
                let p = profileDoc.Value
                {   continent         = continents[ContinentId.value p.continentId].Value.name
                    region            = p.region
                    remoteWork        = p.remoteWork
                    skills            = p.skills
                                        |> List.map (fun s ->
                                            let notes = match s.notes with Some n -> $" ({n})" | None -> ""
                                            $"{s.description}{notes}")
                })
            |> List.ofSeq
    }

/// Success story data access functions
[<RequireQualifiedAccess>]
module Successes =
    
    // Retrieve all success stories  
    let all () = backgroundTask {
        use  session  = querySession ()
        let  citizens = Dict<Guid, CitizenDocument> ()
        let! stories  =
            session.Query<SuccessDocument>()
                .Include((fun s -> s.Value.citizenId :> obj), citizens)
                .OrderByDescending(fun s -> s.Value.recordedOn)
                .ToListAsync ()
        return
            stories
            |> Seq.map (fun storyDoc ->
                let s = storyDoc.Value
                {   id          = s.id
                    citizenId   = s.citizenId
                    citizenName = Citizen.name citizens[CitizenId.value s.citizenId].Value
                    recordedOn  = s.recordedOn
                    fromHere    = s.fromHere
                    hasStory    = Option.isSome s.story
                })
            |> List.ofSeq
    }
    
    /// Find a success story by its ID
    let findById successId = backgroundTask {
        use  session = querySession ()
        let! success = session.LoadAsync<SuccessDocument> (SuccessId.value successId)
        return Document.TryValue success
    }
    
    /// Save a success story
    let save (success : Success) = backgroundTask {
        use session = docSession ()
        session.Store (SuccessDocument success)
        do! session.SaveChangesAsync ()
    }
    