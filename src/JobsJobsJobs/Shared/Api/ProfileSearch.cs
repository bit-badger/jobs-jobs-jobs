namespace JobsJobsJobs.Shared.Api
{
    /// <summary>
    /// The various ways profiles can be searched
    /// </summary>
    public class ProfileSearch
    {
        /// <summary>
        /// Retrieve citizens from this continent
        /// </summary>
        public string? ContinentId { get; set; }

        /// <summary>
        /// Text for a search within a citizen's skills
        /// </summary>
        public string? Skill { get; set; }

        /// <summary>
        /// Text for a search with a citizen's professional biography and experience fields
        /// </summary>
        public string? BioExperience { get; set; }

        /// <summary>
        /// Whether to retrieve citizens who do or do not want remote work
        /// </summary>
        public string RemoteWork { get; set; } = "";

        /// <summary>
        /// Is the search empty?
        /// </summary>
        public bool IsEmptySearch =>
            string.IsNullOrEmpty(ContinentId)
            && string.IsNullOrEmpty(Skill)
            && string.IsNullOrEmpty(BioExperience)
            && string.IsNullOrEmpty(RemoteWork);
    }
}
