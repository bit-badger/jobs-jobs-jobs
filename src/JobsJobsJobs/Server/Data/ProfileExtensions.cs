using JobsJobsJobs.Shared;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Data
{
    /// <summary>
    /// Extensions to the NpgsqlConnection type to support manipulation of profiles
    /// </summary>
    public static class ProfileExtensions
    {
        /// <summary>
        /// Populate a profile object from the given data reader
        /// </summary>
        /// <param name="rdr">The data reader from which values should be obtained</param>
        /// <returns>The populated profile</returns>
        private static Profile ToProfile(NpgsqlDataReader rdr)
        {
            var continentId = ContinentId.Parse(rdr.GetString("continent_id"));
            return new Profile(CitizenId.Parse(rdr.GetString("citizen_id")), rdr.GetBoolean("seeking_employment"),
                rdr.GetBoolean("is_public"), continentId, rdr.GetString("region"), rdr.GetBoolean("remote_work"),
                rdr.GetBoolean("full_time"), new MarkdownString(rdr.GetString("biography")),
                rdr.GetInstant("last_updated_on"),
                rdr.IsDBNull("experience") ? null : new MarkdownString(rdr.GetString("experience")))
            {
                Continent = new Continent(continentId, rdr.GetString("continent_name"))
            };
        }

        /// <summary>
        /// Populate a skill object from the given data reader
        /// </summary>
        /// <param name="rdr">The data reader from which values should be obtained</param>
        /// <returns>The populated skill</returns>
        private static Skill ToSkill(NpgsqlDataReader rdr) =>
            new Skill(SkillId.Parse(rdr.GetString("id")), CitizenId.Parse(rdr.GetString("citizen_id")),
                rdr.GetString("skill"), rdr.IsDBNull("notes") ? null : rdr.GetString("notes"));

        /// <summary>
        /// Retrieve an employment profile by a citizen ID
        /// </summary>
        /// <param name="citizen">The ID of the citizen whose profile should be retrieved</param>
        /// <returns>The profile, or null if it does not exist</returns>
        public static async Task<Profile?> FindProfileByCitizen(this NpgsqlConnection conn, CitizenId citizen)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                @"SELECT p.*, c.name AS continent_name
                    FROM profile p
                        INNER JOIN continent c ON p.continent_id = c.id
                    WHERE citizen_id = @id";
            cmd.Parameters.Add(new NpgsqlParameter("@id", citizen.Id.ToString()));

            using var rdr = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            return await rdr.ReadAsync().ConfigureAwait(false) ? ToProfile(rdr) : null;
        }

        /// <summary>
        /// Save a profile
        /// </summary>
        /// <param name="profile">The profile to be saved</param>
        public static async Task SaveProfile(this NpgsqlConnection conn, Profile profile)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                @"INSERT INTO profile (
                    citizen_id, seeking_employment, is_public, continent_id, region, remote_work, full_time,
                    biography, last_updated_on, experience
                  ) VALUES (
                    @citizen_id, @seeking_employment, @is_public, @continent_id, @region, @remote_work, @full_time,
                    @biography, @last_updated_on, @experience
                  ) ON CONFLICT (citizen_id) DO UPDATE
                    SET seeking_employment = @seeking_employment,
                        is_public          = @is_public,
                        continent_id       = @continent_id,
                        region             = @region,
                        remote_work        = @remote_work,
                        full_time          = @full_time,
                        biography          = @biography,
                        last_updated_on    = @last_updated_on,
                        experience         = @experience
                    WHERE profile.citizen_id = excluded.citizen_id";
            cmd.Parameters.Add(new NpgsqlParameter("@citizen_id", profile.Id.ToString()));
            cmd.Parameters.Add(new NpgsqlParameter("@seeking_employment", profile.SeekingEmployment));
            cmd.Parameters.Add(new NpgsqlParameter("@is_public", profile.IsPublic));
            cmd.Parameters.Add(new NpgsqlParameter("@continent_id", profile.ContinentId.ToString()));
            cmd.Parameters.Add(new NpgsqlParameter("@region", profile.Region));
            cmd.Parameters.Add(new NpgsqlParameter("@remote_work", profile.RemoteWork));
            cmd.Parameters.Add(new NpgsqlParameter("@full_time", profile.FullTime));
            cmd.Parameters.Add(new NpgsqlParameter("@biography", profile.Biography.Text));
            cmd.Parameters.Add(new NpgsqlParameter("@last_updated_on", profile.LastUpdatedOn));
            cmd.Parameters.Add(new NpgsqlParameter("@experience", 
                profile.Experience == null ? DBNull.Value : profile.Experience.Text));

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve all skills for the given citizen
        /// </summary>
        /// <param name="citizenId">The ID of the citizen whose skills should be retrieved</param>
        /// <returns>The skills defined for this citizen</returns>
        public static async Task<IEnumerable<Skill>> FindSkillsByCitizen(this NpgsqlConnection conn,
            CitizenId citizenId)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM skill WHERE citizen_id = @citizen_id";
            cmd.Parameters.Add(new NpgsqlParameter("citizen_id", citizenId.ToString()));

            var result = new List<Skill>();
            using var rdr = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            while (await rdr.ReadAsync().ConfigureAwait(false))
            {
                result.Add(ToSkill(rdr));
            }

            return result;
        }

        /// <summary>
        /// Save a skill
        /// </summary>
        /// <param name="skill">The skill to be saved</param>
        public static async Task SaveSkill(this NpgsqlConnection conn, Skill skill)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                @"INSERT INTO skill (
                    id, citizen_id, skill, notes
                  ) VALUES (
                    @id, @citizen_id, @skill, @notes
                  ) ON CONFLICT (id) DO UPDATE
                    SET skill = @skill,
                        notes = @notes
                    WHERE skill.id = excluded.id";
            cmd.Parameters.Add(new NpgsqlParameter("id", skill.Id.ToString()));
            cmd.Parameters.Add(new NpgsqlParameter("citizen_id", skill.CitizenId.ToString()));
            cmd.Parameters.Add(new NpgsqlParameter("skill", skill.Description));
            cmd.Parameters.Add(new NpgsqlParameter("notes", skill.Notes == null ? DBNull.Value : skill.Notes));

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Delete any skills that are not in the list of current skill IDs
        /// </summary>
        /// <param name="citizenId">The ID of the citizen to whom the skills belong</param>
        /// <param name="ids">The IDs of their current skills</param>
        public static async Task DeleteMissingSkills(this NpgsqlConnection conn, CitizenId citizenId,
            IEnumerable<SkillId> ids)
        {
            if (!ids.Any()) return;

            var count = 0;
            using var cmd = conn.CreateCommand();
            cmd.CommandText = new StringBuilder("DELETE FROM skill WHERE citizen_id = @citizen_id AND id NOT IN (")
                .Append(string.Join(", ", ids.Select(_ => $"@id{count++}").ToArray()))
                .Append(')')
                .ToString();
            cmd.Parameters.Add(new NpgsqlParameter("citizen_id", citizenId.ToString()));
            count = 0;
            foreach (var id in ids) cmd.Parameters.Add(new NpgsqlParameter($"id{count++}", id.ToString()));

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
