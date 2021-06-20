using JobsJobsJobs.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Data
{
    /// <summary>
    /// Extensions to JobsDbContext to support manipulation of job listings
    /// </summary>
    public static class ListingExtensions
    {
        /// <summary>
        /// Find all job listings for the given citizen ID
        /// </summary>
        /// <param name="citizenId">The citizen ID for which job listings should be retrieved</param>
        /// <returns>The job listings entered by the given citizen</returns>
        public static async Task<IEnumerable<Listing>> FindListingsByCitizen(this JobsDbContext db, CitizenId citizenId)
            => await db.Listings.AsNoTracking()
                .Where(l => l.CitizenId == citizenId)
                .ToListAsync().ConfigureAwait(false);
    }
}
