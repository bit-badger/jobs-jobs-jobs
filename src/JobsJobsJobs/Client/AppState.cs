using JobsJobsJobs.Shared;
using Microsoft.JSInterop;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client
{
    /// <summary>
    /// Information about a user
    /// </summary>
    public record UserInfo(CitizenId Id, string Name);

    /// <summary>
    /// Client-side application state for Jobs, Jobs, Jobs
    /// </summary>
    public class AppState
    {
        public event Action OnChange = () => { };

        private UserInfo? _user = null;

        /// <summary>
        /// The information of the currently logged-in user
        /// </summary>
        public UserInfo? User
        {
            get => _user;
            set
            {
                _user = value;
                NotifyChanged();
            }
        }

        private string _jwt = "";

        /// <summary>
        /// The JSON Web Token (JWT) for the currently logged-on user
        /// </summary>
        public string Jwt
        {
            get => _jwt;
            set
            {
                _jwt = value;
                NotifyChanged();
            }
        }

        private IEnumerable<Continent>? _continents = null;

        /// <summary>
        /// Get a list of continents (only retrieves once per application load)
        /// </summary>
        /// <param name="http">The HTTP client to use to obtain continents the first time</param>
        /// <returns>The list of continents</returns>
        /// <exception cref="ApplicationException">If the continents cannot be loaded</exception>
        public async Task<IEnumerable<Continent>> GetContinents(HttpClient http)
        {
            if (_continents == null)
            {
                ServerApi.SetJwt(http, this);
                var continentResult = await ServerApi.RetrieveMany<Continent>(http, "continent/all");

                if (continentResult.IsOk)
                {
                    _continents = continentResult.Ok;
                }
                else
                {
                    throw new ApplicationException($"Could not load continents - {continentResult.Error}");
                }
            }
            return _continents;
        }

        private DateTimeZone? _tz = null;

        /// <summary>
        /// Get the time zone for the current user's browser
        /// </summary>
        /// <param name="js">The JS interop runtime for the application</param>
        /// <returns>The time zone based on the user's browser</returns>
        public async Task<DateTimeZone> GetTimeZone(IJSRuntime js)
        {
            if (_tz == null)
            {
                try
                {
                    _tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(await js.InvokeAsync<string>("getTimeZone"));
                }
                catch (Exception) { }
            }
            if (_tz == null)
            {
                // Either the zone wasn't found, or the user's browser denied us access to it; there's not much to do
                // here but set it to UTC and move on
                _tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Etc/UTC")!;
            }
            return _tz;
        }

        public AppState() { }

        private void NotifyChanged() => OnChange.Invoke();
    }
}
