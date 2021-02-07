using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Data
{
    /// <summary>
    /// Extensions to JobsDbContext to support manipulation of success stories
    /// </summary>
    public static class SuccessExtensions
    {
        /// <summary>
        /// Get a success story by its ID
        /// </summary>
        /// <param name="id">The ID of the story to retrieve</param>
        /// <returns>The success story, if found</returns>
        public static async Task<Success?> FindSuccessById(this JobsDbContext db, SuccessId id) =>
            await db.Successes.AsNoTracking().SingleOrDefaultAsync(s => s.Id == id).ConfigureAwait(false);

        /// <summary>
        /// Get a list of success stories, with the information needed for the list page
        /// </summary>
        /// <returns>A list of success stories, citizen names, and dates</returns>
        public static async Task<IEnumerable<StoryEntry>> AllStories(this JobsDbContext db) =>
            await db.Successes
                .Join(db.Citizens, s => s.CitizenId, c => c.Id, (s, c) => new { Success = s, Citizen = c })
                .OrderByDescending(it => it.Success.RecordedOn)
                .Select(it => new StoryEntry(it.Success.Id, it.Citizen.Id, it.Citizen.CitizenName,
                    it.Success.RecordedOn, it.Success.FromHere, it.Success.Story != null))
                .ToListAsync().ConfigureAwait(false);
    }
}
