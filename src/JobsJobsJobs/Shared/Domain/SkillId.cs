using System.Threading.Tasks;

namespace JobsJobsJobs.Shared
{
    /// <summary>
    /// The ID of a skill
    /// </summary>
    public record SkillId(ShortId Id)
    {
        /// <summary>
        /// Create a new skill ID
        /// </summary>
        /// <returns>A new skill ID</returns>
        public static async Task<SkillId> Create() => new SkillId(await ShortId.Create());

        /// <summary>
        /// Attempt to create a skill ID from a string
        /// </summary>
        /// <param name="id">The prospective ID</param>
        /// <returns>The skill ID</returns>
        /// <exception cref="System.FormatException">If the string is not a valid skill ID</exception>
        public static SkillId Parse(string id) => new SkillId(ShortId.Parse(id));

        public override string ToString() => Id.ToString();
    }
}
