using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public static async Task<Profile?> FindProfileByCitizen(this JobsDbContext db, CitizenId citizenId) =>
            await db.Profiles.AsNoTracking()
                .Include(p => p.Continent)
                .Include(p => p.Skills)
                .SingleOrDefaultAsync(p => p.Id == citizenId)
                .ConfigureAwait(false);

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
        /// <param name="search">The search parameters</param>
        /// <returns>The information for profiles matching the criteria</returns>
        public static async Task<IEnumerable<ProfileSearchResult>> SearchProfiles(this JobsDbContext db,
            ProfileSearch search)
        {
            var query = db.Profiles
                .Join(db.Citizens, p => p.Id, c => c.Id, (p, c) => new { Profile = p, Citizen = c });

            var useIds = false;
            var citizenIds = new List<CitizenId>();

            if (!string.IsNullOrEmpty(search.ContinentId))
            {
                query = query.Where(it => it.Profile.ContinentId == ContinentId.Parse(search.ContinentId));
            }

            if (!string.IsNullOrEmpty(search.RemoteWork))
            {
                query = query.Where(it => it.Profile.RemoteWork == (search.RemoteWork == "yes"));
            }

            if (!string.IsNullOrEmpty(search.Skill))
            {
                useIds = true;
                citizenIds.AddRange(await db.Skills
                    .Where(s => s.Description.ToLower().Contains(search.Skill.ToLower()))
                    .Select(s => s.CitizenId)
                    .ToListAsync().ConfigureAwait(false));
            }

            if (!string.IsNullOrEmpty(search.BioExperience))
            {
                useIds = true;
                citizenIds.AddRange(await db.Profiles
                    .FromSqlRaw("SELECT citizen_id FROM profile WHERE biography ILIKE {0} OR experience ILIKE {0}",
                        $"%{search.BioExperience}%")
                    .Select(p => p.Id)
                    .ToListAsync().ConfigureAwait(false));
            }

            if (useIds)
            {
                query = query.Where(it => citizenIds.Contains(it.Citizen.Id));
            }

            return await query.Select(x => new ProfileSearchResult(x.Citizen.Id, x.Citizen.CitizenName,
                x.Profile.SeekingEmployment, x.Profile.RemoteWork, x.Profile.FullTime, x.Profile.LastUpdatedOn))
                .ToListAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Search public profiles by the given criteria
        /// </summary>
        /// <param name="search">The search parameters</param>
        /// <returns>The information for profiles matching the criteria</returns>
        public static async Task<IEnumerable<PublicSearchResult>> SearchPublicProfiles(this JobsDbContext db,
            PublicSearch search)
        {
            var query = db.Profiles
                .Include(it => it.Continent)
                .Include(it => it.Skills)
                .Where(it => it.IsPublic);

            var useIds = false;
            var citizenIds = new List<CitizenId>();

            if (!string.IsNullOrEmpty(search.ContinentId))
            {
                query = query.Where(it => it.ContinentId == ContinentId.Parse(search.ContinentId));
            }

            if (!string.IsNullOrEmpty(search.Region))
            {
                query = query.Where(it => it.Region.ToLower().Contains(search.Region.ToLower()));
            }

            if (!string.IsNullOrEmpty(search.RemoteWork))
            {
                query = query.Where(it => it.RemoteWork == (search.RemoteWork == "yes"));
            }

            if (!string.IsNullOrEmpty(search.Skill))
            {
                useIds = true;
                citizenIds.AddRange(await db.Skills
                    .Where(s => s.Description.ToLower().Contains(search.Skill.ToLower()))
                    .Select(s => s.CitizenId)
                    .ToListAsync().ConfigureAwait(false));
            }

            if (useIds)
            {
                query = query.Where(it => citizenIds.Contains(it.Id));
            }

            return await query.Select(x => new PublicSearchResult(x.Continent!.Name, x.Region, x.RemoteWork,
                x.Skills.Select(sk => $"{sk.Description} ({sk.Notes})")))
                .ToListAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Delete skills and profile for the given citizen
        /// </summary>
        /// <param name="citizenId">The ID of the citizen whose profile should be deleted</param>
        public static async Task DeleteProfileByCitizen(this JobsDbContext db, CitizenId citizenId)
        {
            var id = citizenId.ToString();
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM jjj.skill WHERE citizen_id = {id}")
                .ConfigureAwait(false);
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM jjj.profile WHERE citizen_id = {id}")
                .ConfigureAwait(false);
        }
    }
}
