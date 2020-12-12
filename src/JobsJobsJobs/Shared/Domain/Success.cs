namespace JobsJobsJobs.Shared
{
    /// <summary>
    /// A record of success finding employment
    /// </summary>
    public record Success(
        SuccessId Id,
        CitizenId CitizenId,
        Milliseconds RecordedOn,
        bool FromHere,
        MarkdownString? Story);
}
