/// Types intended to be shared between the API and the client application
module JobsJobsJobs.Domain.SharedTypes

// fsharplint:disable FieldNames

/// A successful logon
type LogOnSuccess = {
  /// The JSON Web Token (JWT) to use for API access
  jwt       : string
  /// The ID of the logged-in citizen (as a string)
  citizenId : string
  /// The name of the logged-in citizen
  name      : string
  }
