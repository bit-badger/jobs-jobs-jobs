using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain = JobsJobsJobs.Shared;

namespace JobsJobsJobs.Client.Pages.Profile
{
    public partial class View : ComponentBase
    {
        /// <summary>
        /// Whether data for this component is loading
        /// </summary>
        private bool IsLoading { get; set; } = true;

        /// <summary>
        /// The citizen whose profile is being displayed
        /// </summary>
        private Domain.Citizen Citizen { get; set; } = default!;

        /// <summary>
        /// The profile to display
        /// </summary>
        private Domain.Profile Profile { get; set; } = default!;

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
        /// Error messages from data retrieval
        /// </summary>
        private IList<string> ErrorMessages { get; } = new List<string>();

        /// <summary>
        /// The ID of the citizen whose profile should be displayed
        /// </summary>
        [Parameter]
        public string Id { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            ServerApi.SetJwt(http, state);
            var citizenTask = ServerApi.RetrieveOne<Domain.Citizen>(http, $"citizen/get/{Id}");
            var profileTask = ServerApi.RetrieveOne<Domain.Profile>(http, $"profile/get/{Id}");

            await Task.WhenAll(citizenTask, profileTask);

            if (citizenTask.Result.IsOk && citizenTask.Result.Ok != null)
            {
                Citizen = citizenTask.Result.Ok;
            }
            else if (citizenTask.Result.IsOk)
            {
                ErrorMessages.Add("Citizen not found");
            }
            else
            {
                ErrorMessages.Add(citizenTask.Result.Error);
            }

            if (profileTask.Result.IsOk && profileTask.Result.Ok != null)
            {
                Profile = profileTask.Result.Ok;
            }
            else if (profileTask.Result.IsOk)
            {
                ErrorMessages.Add("Profile not found");
            }
            else
            {
                ErrorMessages.Add(profileTask.Result.Error);
            }
            
            IsLoading = false;
        }
    }
}
