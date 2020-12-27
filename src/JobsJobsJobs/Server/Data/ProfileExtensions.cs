using JobsJobsJobs.Shared;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
