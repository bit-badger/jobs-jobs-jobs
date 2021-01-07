using JobsJobsJobs.Shared;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Data
{
    /// <summary>
    /// Data extensions for manipulation of continent objects
    /// </summary>
    public static class ContinentExtensions
    {
        /// <summary>
        /// Retrieve all continents
        /// </summary>
        /// <returns>All continents</returns>
        public static async Task<IEnumerable<Continent>> AllContinents(this JobsDbContext db) =>
            await db.Continents.AsNoTracking().OrderBy(c => c.Name).ToListAsync().ConfigureAwait(false);

        /// <summary>
        /// Retrieve a continent by its ID
        /// </summary>
        /// <param name="continentId">The ID of the continent to retrieve</param>
        /// <returns>The continent matching the ID</returns>
        public static async Task<Continent> FindContinentById(this JobsDbContext db, ContinentId continentId) =>
            await db.Continents.AsNoTracking()
                .SingleAsync(c => c.Id == continentId)
                .ConfigureAwait(false);
    }
}
