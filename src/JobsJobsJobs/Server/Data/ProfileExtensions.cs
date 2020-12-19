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
            return new Profile(CitizenId.Parse(rdr.GetString("id")), rdr.GetBoolean("seeking_employment"),
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
    }
}
