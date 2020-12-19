namespace JobsJobsJobs.Shared.Api
{
    /// <summary>
    /// A successful log on; returns JWT, citizen ID, and display name
    /// </summary>
    public record LogOnSuccess(string Jwt, string Id, string Name)
    {
        /// <summary>
        /// The ID return value as a citizen ID
        /// </summary>
        public CitizenId CitizenId => CitizenId.Parse(Id);
    }
}
