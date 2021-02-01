using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain = JobsJobsJobs.Shared;

namespace JobsJobsJobs.Client.Pages.SuccessStory
{
    public partial class ViewStory : ComponentBase
    {
        /// <summary>
        /// The ID of the success story to display
        /// </summary>
        [Parameter]
        public string Id { get; set; } = default!;

        /// <summary>
        /// The success story to be displayed
        /// </summary>
        private Domain.Success Story { get; set; } = default!;

        /// <summary>
        /// The citizen who authorized this success story
        /// </summary>
        private Domain.Citizen Citizen { get; set; } = default!;

        /// <summary>
        /// Retrieve the success story
        /// </summary>
        /// <param name="errors">The error collection via which errors will be reported</param>
        public async Task RetrieveStory(ICollection<string> errors)
        {
            ServerApi.SetJwt(http, state);
            var story = await ServerApi.RetrieveOne<Domain.Success>(http, $"success/{Id}");

            if (story.IsOk)
            {
                if (story.Ok == null)
                {
                    errors.Add($"Success story {Id} not found");
                }
                else
                {
                    Story = story.Ok;
                    var citizen = await ServerApi.RetrieveOne<Domain.Citizen>(http, $"citizen/get/{Story.CitizenId}");
                    if (citizen.IsOk)
                    {
                        if (citizen.Ok == null)
                        {
                            errors.Add($"Citizen ID {Story.CitizenId} not found");
                        }
                        else
                        {
                            Citizen = citizen.Ok;
                        }
                    }
                    else
                    {
                        errors.Add(citizen.Error);
                    }
                }
            }
            else
            {
                errors.Add(story.Error);
            }

        }
    }
}
