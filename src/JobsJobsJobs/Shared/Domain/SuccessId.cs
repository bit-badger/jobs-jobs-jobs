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

        /// <summary>
        /// Attempt to create a success report ID from a string
        /// </summary>
        /// <param name="id">The prospective ID</param>
        /// <returns>The success report ID</returns>
        /// <exception cref="System.FormatException">If the string is not a valid success report ID</exception>
        public static SuccessId Parse(string id) => new SuccessId(ShortId.Parse(id));

        public override string ToString() => Id.ToString();
    }
}
