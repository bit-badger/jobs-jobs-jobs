using System.Collections.Generic;

namespace JobsJobsJobs.Shared.Api
{
    /// <summary>
    /// A public profile search result
    /// </summary>
    public record PublicSearchResult(
        string Continent,
        string Region,
        bool RemoteWork,
        IEnumerable<string> Skills);
}
