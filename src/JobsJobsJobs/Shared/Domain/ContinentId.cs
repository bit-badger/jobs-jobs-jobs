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

        /// <summary>
        /// Attempt to create a continent ID from a string
        /// </summary>
        /// <param name="id">The prospective ID</param>
        /// <returns>The continent ID</returns>
        /// <exception cref="System.FormatException">If the string is not a valid continent ID</exception>
        public static ContinentId Parse(string id) => new ContinentId(ShortId.Parse(id));

        public override string ToString() => Id.ToString();
    }
}
