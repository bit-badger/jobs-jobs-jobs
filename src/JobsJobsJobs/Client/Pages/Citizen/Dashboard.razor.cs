using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain = JobsJobsJobs.Shared;

namespace JobsJobsJobs.Client.Pages.Citizen
{
    /// <summary>
    /// The first page a user sees after signing in
    /// </summary>
    public partial class Dashboard : ComponentBase
    {
        /// <summary>
        /// Whether the data is being retrieved
        /// </summary>
        private bool RetrievingData { get; set; } = true;

        /// <summary>
        /// The user's profile
        /// </summary>
        private Domain.Profile? Profile { get; set; } = null;

        /// <summary>
        /// The number of profiles
        /// </summary>
        private int ProfileCount { get; set; }

        /// <summary>
        /// Error messages from data access
        /// </summary>
        private IList<string> ErrorMessages { get; } = new List<string>();

        protected override async Task OnInitializedAsync()
        {
            if (state.User != null)
            {
                ServerApi.SetJwt(http, state);
                var profileTask = ServerApi.RetrieveProfile(http, state);
                var profileCountTask = ServerApi.RetrieveOne<Count>(http, "profile/count");

                await Task.WhenAll(profileTask, profileCountTask);

                if (profileTask.Result.IsOk)
                {
                    Profile = profileTask.Result.Ok;
                }
                else
                {
                    ErrorMessages.Add(profileTask.Result.Error);
                }

                if (profileCountTask.Result.IsOk)
                {
                    ProfileCount = profileCountTask.Result.Ok?.Value ?? 0;
                }
                else
                {
                    ErrorMessages.Add(profileCountTask.Result.Error);
                }

                RetrievingData = false;
            }
        }
    }
}
