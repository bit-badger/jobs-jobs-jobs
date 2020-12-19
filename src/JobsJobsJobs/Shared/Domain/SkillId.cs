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
    }
}
