using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Shared
{
    /// <summary>
    /// A component that allows a skill to be edited
    /// </summary>
    public partial class SkillEdit : ComponentBase
    {
        /// <summary>
        /// The skill being edited
        /// </summary>
        [Parameter]
        public SkillForm Skill { get; set; } = default!;

        /// <summary>
        /// Callback used if the remove button is clicked
        /// </summary>
        [Parameter]
        public EventCallback<string> OnRemove { get; set; } = default!;

        /// <summary>
        /// Remove this skill from the skill collection
        /// </summary>
        private Task RemoveMe() => OnRemove.InvokeAsync(Skill.Id);
    }
}
