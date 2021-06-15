using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client
{
    /// <summary>
    /// Functions used to access the API
    /// </summary>
    public static class ServerApi
    {
        /// <summary>
        /// System.Text.Json options configured for NodaTime
        /// </summary>
        private static readonly JsonSerializerOptions _serializerOptions;

        /// <summary>
        /// Static constructor
        /// </summary>
        static ServerApi()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            _serializerOptions = options;
        }

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
        /// Set the JSON Web Token (JWT) bearer header for the given HTTP client
        /// </summary>
        /// <param name="http">The HTTP client whose authentication header should be set</param>
        /// <param name="state">The current application state</param>
        public static void SetJwt(HttpClient http, AppState state)
        {
            if (state.User != null)
            {
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", state.Jwt);
            }
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
                _ when res.IsSuccessStatusCode => Result<Profile?>.AsOk(
                    await res.Content.ReadFromJsonAsync<Profile>(_serializerOptions)),
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

        /// <summary>
        /// Retrieve many items from the given URL
        /// </summary>
        /// <typeparam name="T">The type of item expected</typeparam>
        /// <param name="http">The HTTP client to use for server communication</param>
        /// <param name="url">The API URL to call</param>
        /// <returns>A result with the items, or an error if one occurs</returns>
        /// <remarks>The caller is responsible for setting the JWT on the HTTP client</remarks>
        public static async Task<Result<IEnumerable<T>>> RetrieveMany<T>(HttpClient http, string url)
        {
            try
            {
                var results = await http.GetFromJsonAsync<IEnumerable<T>>($"/api/{url}", _serializerOptions);
                return Result<IEnumerable<T>>.AsOk(results ?? Enumerable.Empty<T>());
            }
            catch (HttpRequestException ex)
            {
                return Result<IEnumerable<T>>.AsError(ex.Message);
            }
            catch (JsonException ex)
            {
                return Result<IEnumerable<T>>.AsError($"Unable to parse result: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieve one item from the given URL
        /// </summary>
        /// <typeparam name="T">The type of item expected</typeparam>
        /// <param name="http">The HTTP client to use for server communication</param>
        /// <param name="url">The API URL to call</param>
        /// <returns>A result with the item (possibly null), or an error if one occurs</returns>
        /// <remarks>The caller is responsible for setting the JWT on the HTTP client</remarks>
        public static async Task<Result<T?>> RetrieveOne<T>(HttpClient http, string url)
        {
            try
            {
                return Result<T?>.AsOk(await http.GetFromJsonAsync<T>($"/api/{url}", _serializerOptions));
            }
            catch (HttpRequestException ex)
            {
                return Result<T?>.AsError(ex.Message);
            }
            catch (JsonException ex)
            {
                return Result<T?>.AsError($"Unable to parse result: {ex.Message}");
            }
        }
    }
}
