using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Pages.Citizen
{
    public partial class Authorized : ComponentBase
    {
        /// <summary>
        /// The message to be displayed to the user
        /// </summary>
        public string Message { get; set; } = "Logging you on with No Agenda Social...";

        /// <summary>
        /// HTTP client for performing API calls
        /// </summary>
        [Inject]
        public HttpClient Http { get; init; } = default!;

        /// <summary>
        /// Navigation manager for getting parameters from the URL
        /// </summary>
        [Inject]
        public NavigationManager Navigation { get; set; } = default!;

        /// <summary>
        /// Application state
        /// </summary>
        [Inject]
        public AppState State { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            // Exchange authorization code for a JWT
            var query = QueryHelpers.ParseQuery(Navigation.ToAbsoluteUri(Navigation.Uri).Query);
            if (query.TryGetValue("code", out var authCode))
            {
                var logOnResult = await Http.GetAsync($"api/citizen/log-on/{authCode}");

                if (logOnResult != null)
                {
                    if (logOnResult.IsSuccessStatusCode)
                    {
                        var logOn = (await logOnResult.Content.ReadFromJsonAsync<LogOnSuccess>())!;
                        State.User = new UserInfo(logOn.CitizenId, logOn.Name);
                        State.Jwt = logOn.Jwt;
                        Navigation.NavigateTo("/citizen/dashboard");
                    }
                    else
                    {
                        var errorMessage = await logOnResult.Content.ReadAsStringAsync();
                        Message = $"Unable to log on with No Agenda Social: {errorMessage}";
                    }
                }
                else
                {
                    Message = "Unable to log on with No Agenda Social. This should never happen; contact @danieljsummers";
                }
            }
            else
            {
                Message = "Did not receive a token from No Agenda Social (perhaps you clicked \"Cancel\"?)";
            }
        }
    }
}
