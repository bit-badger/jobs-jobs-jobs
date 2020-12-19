using System.Threading.Tasks;

namespace JobsJobsJobs.Shared
{
    /// <summary>
    /// The ID of a user (a citizen of Gitmo Nation)
    /// </summary>
    public record CitizenId(ShortId Id)
    {
        /// <summary>
        /// Create a new citizen ID
        /// </summary>
        /// <returns>A new citizen ID</returns>
        public static async Task<CitizenId> Create() => new CitizenId(await ShortId.Create());

        /// <summary>
        /// Attempt to create a citizen ID from a string
        /// </summary>
        /// <param name="id">The prospective ID</param>
        /// <returns>The citizen ID</returns>
        /// <exception cref="System.FormatException">If the string is not a valid citizen ID</exception>
        public static CitizenId Parse(string id) => new CitizenId(ShortId.Parse(id));

        public override string ToString() => Id.ToString();
    }
}
