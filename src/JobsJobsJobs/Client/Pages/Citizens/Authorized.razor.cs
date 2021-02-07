using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Pages.Citizens
{
    public partial class Authorized : ComponentBase
    {
        /// <summary>
        /// The message to be displayed on this page
        /// </summary>
        private string Message { get; set; } = "Logging you on with No Agenda Social...";

        protected override async Task OnInitializedAsync()
        {
            // Exchange authorization code for a JWT
            var query = QueryHelpers.ParseQuery(nav.ToAbsoluteUri(nav.Uri).Query);
            if (query.TryGetValue("code", out var authCode))
            {
                var logOnResult = await ServerApi.LogOn(http, authCode);

                if (logOnResult.IsOk)
                {
                    var logOn = logOnResult.Ok;
                    state.User = new UserInfo(logOn.CitizenId, logOn.Name);
                    state.Jwt = logOn.Jwt;
                    nav.NavigateTo("/citizen/dashboard");
                }
                else
                {
                    Message = logOnResult.Error;
                }
            }
            else
            {
                Message = "Did not receive a token from No Agenda Social (perhaps you clicked \"Cancel\"?)";
            }
        }
    }
}
