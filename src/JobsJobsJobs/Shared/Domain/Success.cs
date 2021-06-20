using NodaTime;

namespace JobsJobsJobs.Shared
{
    /// <summary>
    /// A record of success finding employment
    /// </summary>
    public record Success(
        SuccessId Id,
        CitizenId CitizenId,
        Instant RecordedOn,
        bool FromHere,
        string Source,
        MarkdownString? Story);
}
