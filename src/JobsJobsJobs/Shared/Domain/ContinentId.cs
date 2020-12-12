using System.Threading.Tasks;

namespace JobsJobsJobs.Shared
{
    /// <summary>
    /// The ID of a continent
    /// </summary>
    public record ContinentId(ShortId Id)
    {
        /// <summary>
        /// Create a new continent ID
        /// </summary>
        /// <returns>A new continent ID</returns>
        public static async Task<ContinentId> Create() => new ContinentId(await ShortId.Create());
    }
}
