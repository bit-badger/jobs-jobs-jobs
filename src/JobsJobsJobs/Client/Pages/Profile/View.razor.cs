using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain = JobsJobsJobs.Shared;

namespace JobsJobsJobs.Client.Pages.Profile
{
    public partial class View : ComponentBase
    {
        private bool IsLoading { get; set; } = true;

        private Domain.Citizen? Citizen { get; set; }

        private Domain.Profile? Profile { get; set; }

        private IList<string> ErrorMessages { get; } = new List<string>();

        [Parameter]
        public string Id { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            ServerApi.SetJwt(http, state);
            var citizenTask = ServerApi.RetrieveOne<Domain.Citizen>(http, $"/api/citizen/{Id}");
            var profileTask = ServerApi.RetrieveOne<Domain.Profile>(http, $"/api/profile/get/{Id}");

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
        }
    }
}
