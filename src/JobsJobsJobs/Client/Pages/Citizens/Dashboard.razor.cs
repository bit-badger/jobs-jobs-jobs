using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Pages.Citizens
{
    /// <summary>
    /// The first page a user sees after signing in
    /// </summary>
    public partial class Dashboard : ComponentBase
    {
        /// <summary>
        /// The user's profile
        /// </summary>
        private Profile? Profile { get; set; } = null;

        /// <summary>
        /// The number of profiles
        /// </summary>
        private int ProfileCount { get; set; }

        /// <summary>
        /// Load the user's profile information
        /// </summary>
        /// <param name="errors">A collection to report errors that may be encountered</param>
        public async Task LoadProfile(ICollection<string> errors)
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
                    errors.Add(profileTask.Result.Error);
                }

                if (profileCountTask.Result.IsOk)
                {
                    ProfileCount = profileCountTask.Result.Ok?.Value ?? 0;
                }
                else
                {
                    errors.Add(profileCountTask.Result.Error);
                }
            }
        }
    }
}
