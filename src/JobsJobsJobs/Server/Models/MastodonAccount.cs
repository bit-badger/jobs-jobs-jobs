using System.Text.Json.Serialization;

namespace JobsJobsJobs.Server.Models
{
    /// <summary>
    /// The variables we need from the account information we get from No Agenda Social
    /// </summary>
    public class MastodonAccount
    {
        /// <summary>
        /// The user name (what we store as naUser)
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; } = "";

        /// <summary>
        /// The account name; will be the same as username for local (non-federated) accounts
        /// </summary>
        [JsonPropertyName("acct")]
        public string AccountName { get; set; } = "";

        /// <summary>
        /// The user's display name as it currently shows on No Agenda Social
        /// </summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = "";

        /// <summary>
        /// The user's profile URL
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = "";
    }
}
