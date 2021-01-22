namespace JobsJobsJobs.Shared.Api
{
    /// <summary>
    /// The data required to provide a success story
    /// </summary>
    public class StoryForm
    {
        /// <summary>
        /// The ID of this story
        /// </summary>
        public string Id { get; set; } = "new";

        /// <summary>
        /// Whether the employment was obtained from Jobs, Jobs, Jobs
        /// </summary>
        public bool FromHere { get; set; } = false;

        /// <summary>
        /// The success story
        /// </summary>
        public string Story { get; set; } = "";
    }
}
