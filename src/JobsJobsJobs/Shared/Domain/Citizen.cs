namespace JobsJobsJobs.Shared
{
    /// <summary>
    /// A user of Jobs, Jobs, Jobs
    /// </summary>
    public record Citizen(
        CitizenId Id,
        string NaUser,
        string DisplayName,
        string ProfileUrl,
        Milliseconds JoinedOn,
        Milliseconds LastSeenOn);
}
