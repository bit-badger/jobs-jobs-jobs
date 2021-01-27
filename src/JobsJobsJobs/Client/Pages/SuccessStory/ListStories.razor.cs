using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Pages.SuccessStory
{
    public partial class ListStories : ComponentBase
    {
        /// <summary>
        /// The story entries
        /// </summary>
        private IEnumerable<StoryEntry> Stories { get; set; } = default!;

        /// <summary>
        /// Load all success stories
        /// </summary>
        /// <param name="errors">The collection into which errors can be reported</param>
        public async Task LoadStories(ICollection<string> errors)
        {
            ServerApi.SetJwt(http, state);
            var stories = await ServerApi.RetrieveMany<StoryEntry>(http, "success/list");

            if (stories.IsOk)
            {
                Stories = stories.Ok;
            }
            else
            {
                errors.Add(stories.Error);
            }
        }
    }
}
