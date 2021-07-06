using JobsJobsJobs.Shared;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Pages.Listings
{
    public partial class Mine : ComponentBase
    {
        /// <summary>
        /// The job listings entered by the current user
        /// </summary>
        private IEnumerable<Listing> Listings { get; set; } = Enumerable.Empty<Listing>();

        /// <summary>
        /// Load the user's job listings
        /// </summary>
        /// <param name="errors">Error collection for possible problems</param>
        private async Task OnLoad(ICollection<string> errors)
        {
            var listings = await ServerApi.RetrieveMany<Listing>(http, "listing/mine");

            if (listings.IsOk)
            {
                Listings = listings.Ok;
            }
            else
            {
                errors.Add(listings.Error);
            }
        }
    }
}
