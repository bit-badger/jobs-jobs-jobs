using NodaTime;

namespace JobsJobsJobs.Shared.Api
{
    /// <summary>
    /// An entry in the list of success stories
    /// </summary>
    public record StoryEntry(SuccessId Id, CitizenId CitizenId, string CitizenName, Instant RecordedOn);
}
