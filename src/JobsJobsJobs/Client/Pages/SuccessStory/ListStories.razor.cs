using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Pages.SuccessStory
{
    public partial class ListStories : ComponentBase
    {
        /// <summary>
        /// Whether we are still loading data
        /// </summary>
        private bool Loading { get; set; } = true;

        /// <summary>
        /// The story entries
        /// </summary>
        private IEnumerable<StoryEntry> Stories { get; set; } = default!;

        /// <summary>
        /// Error messages encountered
        /// </summary>
        private IList<string> ErrorMessages { get; set; } = new List<string>();

        protected override async Task OnInitializedAsync()
        {
            ServerApi.SetJwt(http, state);
            var stories = await ServerApi.RetrieveMany<StoryEntry>(http, "success/list");

            if (stories.IsOk)
            {
                Stories = stories.Ok;
            }
            else
            {
                ErrorMessages.Add(stories.Error);
            }

            Loading = false;
        }
    }
}
