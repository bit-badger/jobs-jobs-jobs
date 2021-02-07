using JobsJobsJobs.Shared;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Pages.Profiles
{
    public partial class View : ComponentBase
    {
        /// <summary>
        /// The citizen whose profile is being displayed
        /// </summary>
        private Citizen Citizen { get; set; } = default!;

        /// <summary>
        /// The profile to display
        /// </summary>
        private Profile Profile { get; set; } = default!;

        /// <summary>
        /// The work types for the top of the page
        /// </summary>
        private MarkupString WorkTypes
        {
            get
            {
                IEnumerable<string> parts()
                {
                    if (Profile.SeekingEmployment)
                    {
                        yield return "<strong><em>CURRENTLY SEEKING EMPLOYMENT</em></strong>";
                    }
                    else
                    {
                        yield return "Not actively seeking employment";
                    }
                    yield return $"{(Profile.FullTime ? "I" : "Not i")}nterested in full-time employment";
                    yield return $"{(Profile.RemoteWork ? "I" : "Not i")}nterested in remote opportunities";
                }

                return new MarkupString(string.Join(" &bull; ", parts()));
            }
        }

        /// <summary>
        /// The ID of the citizen whose profile should be displayed
        /// </summary>
        [Parameter]
        public string Id { get; set; } = default!;

        /// <summary>
        /// Retrieve the requested profile
        /// </summary>
        /// <param name="errors">A collection to report errors that may occur</param>
        public async Task RetrieveProfile(ICollection<string> errors)
        {
            ServerApi.SetJwt(http, state);
            var citizenTask = ServerApi.RetrieveOne<Citizen>(http, $"citizen/get/{Id}");
            var profileTask = ServerApi.RetrieveOne<Profile>(http, $"profile/get/{Id}");

            await Task.WhenAll(citizenTask, profileTask);

            if (citizenTask.Result.IsOk && citizenTask.Result.Ok != null)
            {
                Citizen = citizenTask.Result.Ok;
            }
            else if (citizenTask.Result.IsOk)
            {
                errors.Add("Citizen not found");
            }
            else
            {
                errors.Add(citizenTask.Result.Error);
            }

            if (profileTask.Result.IsOk && profileTask.Result.Ok != null)
            {
                Profile = profileTask.Result.Ok;
            }
            else if (profileTask.Result.IsOk)
            {
                errors.Add("Profile not found");
            }
            else
            {
                errors.Add(profileTask.Result.Error);
            }
        }
    }
}
