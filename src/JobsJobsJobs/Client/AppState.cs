using JobsJobsJobs.Shared;

namespace JobsJobsJobs.Client
{
    /// <summary>
    /// Information about a user
    /// </summary>
    public record UserInfo(CitizenId Id, string Name);

    /// <summary>
    /// Client-side application state for Jobs, Jobs, Jobs
    /// </summary>
    public class AppState
    {
        /// <summary>
        /// The information of the currently logged-in user
        /// </summary>
        public UserInfo? User { get; set; } = null;

        /// <summary>
        /// The JSON Web Token (JWT) for the currently logged-on user
        /// </summary>
        public string Jwt { get; set; } = "";

        public AppState() { }
    }
}
