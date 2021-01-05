using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

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
        private Profile? Profile { get; set; } = null;

        /// <summary>
        /// The number of skills in the user's profile
        /// </summary>
        private long SkillCount { get; set; } = 0L;

        /// <summary>
        /// The number of profiles
        /// </summary>
        private long ProfileCount { get; set; } = 0L;

        /// <summary>
        /// Error messages from data access
        /// </summary>
        private IList<string> ErrorMessages { get; } = new List<string>();

        /// <summary>
        /// The HTTP client to use for API access
        /// </summary>
        [Inject]
        public HttpClient Http { get; set; } = default!;

        /// <summary>
        /// The current application state
        /// </summary>
        [Inject]
        public AppState State { get; set; } = default!;


        protected override async Task OnInitializedAsync()
        {
            if (State.User != null)
            {
                ServerApi.SetJwt(Http, State);
                var profileTask = ServerApi.RetrieveProfile(Http, State);
                var profileCountTask = ServerApi.RetrieveOne<Count>(Http, "profile/count");
                var skillCountTask = ServerApi.RetrieveOne<Count>(Http, "profile/skill-count");

                await Task.WhenAll(profileTask, profileCountTask, skillCountTask);

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

                if (skillCountTask.Result.IsOk)
                {
                    SkillCount = skillCountTask.Result.Ok?.Value ?? 0;
                }
                else
                {
                    ErrorMessages.Add(skillCountTask.Result.Error);
                }

                RetrievingData = false;
            }
        }

    }
}
