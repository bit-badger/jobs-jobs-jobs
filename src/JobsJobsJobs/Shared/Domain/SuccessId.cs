using System.Threading.Tasks;

namespace JobsJobsJobs.Shared
{
    /// <summary>
    /// The ID of a success report
    /// </summary>
    public record SuccessId(ShortId Id)
    {
        /// <summary>
        /// Create a new success report ID
        /// </summary>
        /// <returns>A new success report ID</returns>
        public static async Task<SuccessId> Create() => new SuccessId(await ShortId.Create());
    }
}
