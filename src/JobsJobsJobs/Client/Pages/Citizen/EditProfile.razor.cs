using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Pages.Citizen
{
    /// <summary>
    /// Profile edit page (called EditProfile so as not to create naming conflicts)
    /// </summary>
    public partial class EditProfile : ComponentBase
    {
        /// <summary>
        /// The form for this page
        /// </summary>
        private ProfileForm ProfileForm { get; set; } = new ProfileForm();

        /// <summary>
        /// All continents
        /// </summary>
        private IEnumerable<Continent> Continents { get; set; } = Enumerable.Empty<Continent>();

        /// <summary>
        /// Error message from API access
        /// </summary>
        private string ErrorMessage { get; set; } = "";

        /// <summary>
        /// HTTP client instance to use for API access
        /// </summary>
        [Inject]
        private HttpClient Http { get; set; } = default!;

        /// <summary>
        /// Application state
        /// </summary>
        [Inject]
        private AppState State { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            ServerApi.SetJwt(Http, State);
            var continentResult = await ServerApi.AllContinents(Http, State);
            if (continentResult.IsOk)
            {
                Continents = continentResult.Ok;
            }
            else
            {
                ErrorMessage = continentResult.Error;
            }

            var result = await ServerApi.RetrieveProfile(Http, State);
            if (result.IsOk)
            {
                System.Console.WriteLine($"Result is null? {result.Ok == null}");
                ProfileForm = (result.Ok == null) ? new ProfileForm() : ProfileForm.FromProfile(result.Ok);
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }

        public async Task SaveProfile()
        {
            var res = await Http.PostAsJsonAsync("/api/profile/save", ProfileForm);
            if (res.IsSuccessStatusCode)
            {
                // TODO: success notification
            }
            else
            {
                // TODO: probably not the best way to handle this...
                ErrorMessage = await res.Content.ReadAsStringAsync();
            }
        }

    }
}
