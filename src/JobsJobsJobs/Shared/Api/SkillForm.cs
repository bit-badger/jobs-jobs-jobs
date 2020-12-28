using System.ComponentModel.DataAnnotations;

namespace JobsJobsJobs.Shared.Api
{
    /// <summary>
    /// The fields required for a skill
    /// </summary>
    public class SkillForm
    {
        /// <summary>
        /// The ID of this skill
        /// </summary>
        [Required]
        public string Id { get; set; } = "";

        /// <summary>
        /// The description of the skill
        /// </summary>
        [StringLength(100)]
        public string Description { get; set; } = "";

        /// <summary>
        /// Notes regarding the skill
        /// </summary>
        [StringLength(100)]
        public string? Notes { get; set; } = null;
    }
}
