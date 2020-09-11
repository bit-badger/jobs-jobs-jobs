module JobsJobsJobs.Api.ViewModels

// fsharplint:disable RecordFieldNames MemberNames

/// View models uses for /api/citizen routes
module Citizen =

  open FSharp.Json

  /// The payload for the log on route
  type LogOn = {
    /// The access token obtained from No Agenda Social
    accessToken : string
    }

  /// The variables we need from the account information we get from No Agenda Social
  type MastodonAccount = {
    /// The user name (what we store as naUser)
    username    : string
    /// The account name; will be the same as username for local (non-federated) accounts
    acct        : string
    /// The user's display name as it currently shows on No Agenda Social
    [<JsonField "display_name">]
    displayName : string
    /// The user's profile URL
    url         : string
    }
