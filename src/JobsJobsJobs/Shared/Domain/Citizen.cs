using NodaTime;

namespace JobsJobsJobs.Shared
{
    /// <summary>
    /// A user of Jobs, Jobs, Jobs
    /// </summary>
    public record Citizen(
        CitizenId Id,
        string NaUser,
        string? DisplayName,
        string? RealName,
        string ProfileUrl,
        Instant JoinedOn,
        Instant LastSeenOn)
    {
        /// <summary>
        /// The user's name by which they should be known
        /// </summary>
        public string CitizenName => RealName ?? DisplayName ?? NaUser;
    }
}
