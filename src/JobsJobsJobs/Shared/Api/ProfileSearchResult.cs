using NodaTime;

namespace JobsJobsJobs.Shared.Api
{
    /// <summary>
    /// A user matching the profile search
    /// </summary>
    public record ProfileSearchResult(
        CitizenId CitizenId,
        string DisplayName,
        bool SeekingEmployment,
        bool RemoteWork,
        bool FullTime,
        Instant LastUpdated);
}
