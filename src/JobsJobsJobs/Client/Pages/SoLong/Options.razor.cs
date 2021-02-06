using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Pages.SoLong
{
    public partial class Options : ComponentBase
    {
        /// <summary>
        /// Extract an error phrase from a response similar to <code>404 - Not Found</code>
        /// </summary>
        /// <param name="response">The HTTP response</param>
        /// <returns>The formatted error code</returns>
        private static string ErrorPhrase(HttpResponseMessage response) =>
            $"{response.StatusCode}{(string.IsNullOrEmpty(response.ReasonPhrase) ? "" : $" - {response.ReasonPhrase }")}";

        /// <summary>
        /// Delete the profile only; redirect to home page on success
        /// </summary>
        private async Task DeleteProfile()
        {
            ServerApi.SetJwt(http, state);
            var result = await http.DeleteAsync("/api/profile/");
            if (result.IsSuccessStatusCode)
            {
                toast.ShowSuccess("Profile Deleted Successfully");
                nav.NavigateTo("/citizen/dashboard");
            }
            else
            {
                toast.ShowError(ErrorPhrase(result));
            }
        }

        /// <summary>
        /// Delete everything pertaining to the user's account
        /// </summary>
        private async Task DeleteAccount()
        {
            ServerApi.SetJwt(http, state);
            var result = await http.DeleteAsync("/api/citizen/");
            if (result.IsSuccessStatusCode)
            {
                state.Jwt = "";
                state.User = null;
                nav.NavigateTo("/so-long/success");
            }
            else
            {
                toast.ShowError(ErrorPhrase(result));
            }
        }
    }
}
