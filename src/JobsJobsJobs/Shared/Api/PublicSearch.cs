namespace JobsJobsJobs.Shared.Api
{
    /// <summary>
    /// The parameters for a public job search
    /// </summary>
    public class PublicSearch
    {
        /// <summary>
        /// Retrieve citizens from this continent
        /// </summary>
        public string? ContinentId { get; set; }

        /// <summary>
        /// Retrieve citizens from this region
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// Text for a search within a citizen's skills
        /// </summary>
        public string? Skill { get; set; }

        /// <summary>
        /// Whether to retrieve citizens who do or do not want remote work
        /// </summary>
        public string RemoteWork { get; set; } = "";

        /// <summary>
        /// Is the search empty?
        /// </summary>
        public bool IsEmptySearch =>
            string.IsNullOrEmpty(ContinentId)
            && string.IsNullOrEmpty(Region)
            && string.IsNullOrEmpty(Skill)
            && string.IsNullOrEmpty(RemoteWork);
    }
}
