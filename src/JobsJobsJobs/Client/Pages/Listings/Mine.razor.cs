using JobsJobsJobs.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Pages.Listings
{
    public partial class Mine : ComponentBase
    {
        /// <summary>
        /// Whether the page is loading data
        /// </summary>
        private bool Loading { get; set; } = true;

        /// <summary>
        /// Error messages encountered while searching for profiles
        /// </summary>
        private IList<string> ErrorMessages { get; } = new List<string>();

        /// <summary>
        /// The job listings entered by the current user
        /// </summary>
        private IEnumerable<Listing> Listings { get; set; } = Enumerable.Empty<Listing>();

        protected override async Task OnInitializedAsync()
        {
            var listings = await ServerApi.RetrieveMany<Listing>(http, "listing/mine");

            if (listings.IsOk)
            {
                Listings = listings.Ok;
            }
            else
            {
                ErrorMessages.Add(listings.Error);
            }
        }
    }
}
