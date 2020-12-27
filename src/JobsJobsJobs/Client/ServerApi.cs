using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client
{
    /// <summary>
    /// Functions used to access the API
    /// </summary>
    public static class ServerApi
    {
        /// <summary>
        /// Create an API URL
        /// </summary>
        /// <param name="url">The URL to append to the API base URL</param>
        /// <returns>The full URL to be used in HTTP requests</returns>
        private static string ApiUrl(string url) => $"/api/{url}";

        /// <summary>
        /// Create an HTTP request with an authorization header
        /// </summary>
        /// <param name="state">The current application state</param>
        /// <param name="url">The URL for the request (will be appended to the API root)</param>
        /// <param name="method">The request method (optional, defaults to GET)</param>
        /// <returns>A request with the header attached, ready for further manipulation</returns>
        private static HttpRequestMessage WithHeader(AppState state, string url, HttpMethod? method = null)
        {
            var req = new HttpRequestMessage(method ?? HttpMethod.Get, ApiUrl(url));
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", state.Jwt);
            return req;
        }

        /// <summary>
        /// Log on a user with the authorization code received from No Agenda Social
        /// </summary>
        /// <param name="http">The HTTP client to use for server communication</param>
        /// <param name="authCode">The authorization code received from NAS</param>
        /// <returns>The log on details if successful, an error if not</returns>
        public static async Task<Result<LogOnSuccess>> LogOn(HttpClient http, string authCode)
        {
            try
            {
                var logOn = await http.GetFromJsonAsync<LogOnSuccess>(ApiUrl($"citizen/log-on/{authCode}"));
                if (logOn == null) {
                    return Result<LogOnSuccess>.AsError(
                        "Unable to log on with No Agenda Social. This should never happen; contact @danieljsummers");
                }
                return Result<LogOnSuccess>.AsOk(logOn);
            }
            catch (HttpRequestException ex)
            {
                return Result<LogOnSuccess>.AsError($"Unable to log on with No Agenda Social: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieve a citizen's profile
        /// </summary>
        /// <param name="http">The HTTP client to use for server communication</param>
        /// <param name="state">The current application state</param>
        /// <returns>The citizen's profile, null if it is not found, or an error message if one occurs</returns>
        public static async Task<Result<Profile?>> RetrieveProfile(HttpClient http, AppState state)
        {
            var req = WithHeader(state, "profile/");
            var res = await http.SendAsync(req);
            return true switch
            {
                _ when res.StatusCode == HttpStatusCode.NoContent => Result<Profile?>.AsOk(null),
                _ when res.IsSuccessStatusCode => Result<Profile?>.AsOk(await res.Content.ReadFromJsonAsync<Profile>()),
                _ => Result<Profile?>.AsError(await res.Content.ReadAsStringAsync()),
            };
        }

        /// <summary>
        /// Retrieve all continents
        /// </summary>
        /// <param name="http">The HTTP client to use for server communication</param>
        /// <param name="state">The current application state</param>
        /// <returns>The continents, or an error message if one occurs</returns>
        public static async Task<Result<IEnumerable<Continent>>> AllContinents(HttpClient http, AppState state)
        {
            var req = WithHeader(state, "continent/all");
            var res = await http.SendAsync(req);
            if (res.IsSuccessStatusCode) {
                var continents = await res.Content.ReadFromJsonAsync<IEnumerable<Continent>>();
                return Result<IEnumerable<Continent>>.AsOk(continents ?? Enumerable.Empty<Continent>());
            }
            return Result<IEnumerable<Continent>>.AsError(await res.Content.ReadAsStringAsync());
        }
    }
}
