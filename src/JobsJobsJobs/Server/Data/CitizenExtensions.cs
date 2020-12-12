using JobsJobsJobs.Shared;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Data
{
    /// <summary>
    /// Extensions to the NpgslConnection type supporting the manipulation of citizens
    /// </summary>
    public static class CitizenExtensions
    {
        /// <summary>
        /// Populate a citizen object from the given data reader
        /// </summary>
        /// <param name="rdr">The data reader from which the values should be obtained</param>
        /// <returns>A populated citizen</returns>
        private static Citizen ToCitizen(NpgsqlDataReader rdr) =>
            new Citizen(CitizenId.Parse(rdr.GetString("id")), rdr.GetString("na_user"), rdr.GetString("display_name"),
                rdr.GetString("profile_url"), new Milliseconds(rdr.GetInt64("joined_on")),
                new Milliseconds(rdr.GetInt64("last_seen_on")));

        /// <summary>
        /// Retrieve a citizen by their No Agenda Social user name
        /// </summary>
        /// <param name="naUser">The NAS user name</param>
        /// <returns>The citizen, or null if not found</returns>
        public static async Task<Citizen?> FindCitizenByNAUser(this NpgsqlConnection conn, string naUser)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM citizen WHERE na_user = @na_user";
            cmd.Parameters.Add(new NpgsqlParameter("@na_user", naUser));

            using NpgsqlDataReader rdr = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            if (await rdr.ReadAsync().ConfigureAwait(false))
            {
                return ToCitizen(rdr);
            }
            return null;
        }

        /// <summary>
        /// Add a citizen
        /// </summary>
        /// <param name="citizen">The citizen to be added</param>
        public static async Task AddCitizen(this NpgsqlConnection conn, Citizen citizen)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                @"INSERT INTO citizen (
                    na_user, display_name, profile_url, joined_on, last_seen_on, id
                ) VALUES(
                    @na_user, @display_name, @profile_url, @joined_on, @last_seen_on, @id
                )";
            cmd.Parameters.Add(new NpgsqlParameter("@id", citizen.Id.ToString()));
            cmd.Parameters.Add(new NpgsqlParameter("@na_user", citizen.NaUser));
            cmd.Parameters.Add(new NpgsqlParameter("@display_name", citizen.DisplayName));
            cmd.Parameters.Add(new NpgsqlParameter("@profile_url", citizen.ProfileUrl));
            cmd.Parameters.Add(new NpgsqlParameter("@joined_on", citizen.JoinedOn.Millis));
            cmd.Parameters.Add(new NpgsqlParameter("@last_seen_on", citizen.LastSeenOn.Millis));

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Update a citizen after they have logged on (update last seen, sync display name)
        /// </summary>
        /// <param name="citizen">The updated citizen</param>
        public static async Task UpdateCitizenOnLogOn(this NpgsqlConnection conn, Citizen citizen)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                @"UPDATE citizen
                     SET display_name = @display_name,
                         last_seen_on = @last_seen_on
                   WHERE id = @id";
            cmd.Parameters.Add(new NpgsqlParameter("@id", citizen.Id.ToString()));
            cmd.Parameters.Add(new NpgsqlParameter("@display_name", citizen.DisplayName));
            cmd.Parameters.Add(new NpgsqlParameter("@last_seen_on", citizen.LastSeenOn.Millis));

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
