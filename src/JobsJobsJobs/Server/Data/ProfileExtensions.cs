using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Data
{
    /// <summary>
    /// Extensions to JobsDbContext to support manipulation of profiles
    /// </summary>
    public static class ProfileExtensions
    {
        /// <summary>
        /// Retrieve an employment profile by a citizen ID
        /// </summary>
        /// <param name="citizenId">The ID of the citizen whose profile should be retrieved</param>
        /// <returns>The profile, or null if it does not exist</returns>
        public static async Task<Profile?> FindProfileByCitizen(this JobsDbContext db, CitizenId citizenId)
        {
            var profile = await db.Profiles.AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == citizenId)
                .ConfigureAwait(false);

            if (profile != null)
            {
                return profile with
                {
                    Continent = await db.FindContinentById(profile.ContinentId).ConfigureAwait(false),
                    Skills = (await db.FindSkillsByCitizen(citizenId).ConfigureAwait(false)).ToArray()
                };
            }

            return null;
        }

        /// <summary>
        /// Save a profile
        /// </summary>
        /// <param name="profile">The profile to be saved</param>
        public static async Task SaveProfile(this JobsDbContext db, Profile profile)
        {
            if (await db.Profiles.CountAsync(p => p.Id == profile.Id).ConfigureAwait(false) == 0)
            {
                await db.AddAsync(profile).ConfigureAwait(false);
            }
            else
            {
                db.Entry(profile).State = EntityState.Modified;
            }
        }

        /// <summary>
        /// Retrieve all skills for the given citizen
        /// </summary>
        /// <param name="citizenId">The ID of the citizen whose skills should be retrieved</param>
        /// <returns>The skills defined for this citizen</returns>
        public static async Task<IEnumerable<Skill>> FindSkillsByCitizen(this JobsDbContext db, CitizenId citizenId) =>
            await db.Skills.AsNoTracking()
                .Where(s => s.CitizenId == citizenId)
                .ToListAsync().ConfigureAwait(false);

        /// <summary>
        /// Save a skill
        /// </summary>
        /// <param name="skill">The skill to be saved</param>
        public static async Task SaveSkill(this JobsDbContext db, Skill skill)
        {
            if (await db.Skills.CountAsync(s => s.Id == skill.Id).ConfigureAwait(false) == 0)
            {
                await db.Skills.AddAsync(skill).ConfigureAwait(false);
            }
            else
            {
                db.Entry(skill).State = EntityState.Modified;
            }
        }

        /// <summary>
        /// Delete any skills that are not in the list of current skill IDs
        /// </summary>
        /// <param name="citizenId">The ID of the citizen to whom the skills belong</param>
        /// <param name="ids">The IDs of their current skills</param>
        public static async Task DeleteMissingSkills(this JobsDbContext db, CitizenId citizenId,
            IEnumerable<SkillId> ids)
        {
            if (!ids.Any()) return;

            db.Skills.RemoveRange(await db.Skills.AsNoTracking()
                .Where(s => !ids.Contains(s.Id)).ToListAsync()
                .ConfigureAwait(false));
        }

        /// <summary>
        /// Get a count of the citizens with profiles
        /// </summary>
        /// <returns>The number of citizens with profiles</returns>
        public static async Task<int> CountProfiles(this JobsDbContext db) =>
            await db.Profiles.CountAsync().ConfigureAwait(false);

        /// <summary>
        /// Count the skills for the given citizen
        /// </summary>
        /// <param name="citizenId">The ID of the citizen whose skills should be counted</param>
        /// <returns>The count of skills for the given citizen</returns>
        public static async Task<int> CountSkillsByCitizen(this JobsDbContext db, CitizenId citizenId) =>
            await db.Skills.CountAsync(s => s.CitizenId == citizenId).ConfigureAwait(false);

        /// <summary>
        /// Search profiles by the given criteria
        /// </summary>
        //  TODO: A criteria parameter!
        /// <returns>The information for profiles matching the criteria</returns>
        public static async Task<IEnumerable<ProfileSearchResult>> SearchProfiles(this JobsDbContext db)
        {
            return await db.Profiles
                .Join(db.Citizens, p => p.Id, c => c.Id, (p, c) => new { Profile = p, Citizen = c })
                .Select(x => new ProfileSearchResult(x.Citizen.Id, x.Citizen.DisplayName, x.Profile.SeekingEmployment,
                    x.Profile.RemoteWork, x.Profile.FullTime, x.Profile.LastUpdatedOn))
                .ToListAsync().ConfigureAwait(false);
        }
    }
}
