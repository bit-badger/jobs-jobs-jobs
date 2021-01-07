using JobsJobsJobs.Shared;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Data
{
    /// <summary>
    /// Extensions to JobsDbContext supporting the manipulation of citizens
    /// </summary>
    public static class CitizenExtensions
    {
        /// <summary>
        /// Retrieve a citizen by their Jobs, Jobs, Jobs ID
        /// </summary>
        /// <param name="citizenId">The ID of the citizen to retrieve</param>
        /// <returns>The citizen, or null if not found</returns>
        public static async Task<Citizen?> FindCitizenById(this JobsDbContext db, CitizenId citizenId) =>
            await db.Citizens.AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == citizenId)
                .ConfigureAwait(false);

        /// <summary>
        /// Retrieve a citizen by their No Agenda Social user name
        /// </summary>
        /// <param name="naUser">The NAS user name</param>
        /// <returns>The citizen, or null if not found</returns>
        public static async Task<Citizen?> FindCitizenByNAUser(this JobsDbContext db, string naUser) =>
            await db.Citizens.AsNoTracking()
                .SingleOrDefaultAsync(c => c.NaUser == naUser)
                .ConfigureAwait(false);

        /// <summary>
        /// Add a citizen
        /// </summary>
        /// <param name="citizen">The citizen to be added</param>
        public static async Task AddCitizen(this JobsDbContext db, Citizen citizen) =>
            await db.Citizens.AddAsync(citizen);

        /// <summary>
        /// Update a citizen after they have logged on (update last seen, sync display name)
        /// </summary>
        /// <param name="citizen">The updated citizen</param>
        public static void UpdateCitizen(this JobsDbContext db, Citizen citizen) =>
            db.Entry(citizen).State = EntityState.Modified;
    }
}
