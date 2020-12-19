using JobsJobsJobs.Shared;
using System;

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

        public AppState() { }

        private void NotifyChanged() => OnChange.Invoke();
    }
}
