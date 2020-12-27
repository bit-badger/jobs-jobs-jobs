using JobsJobsJobs.Shared;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Data
{
    /// <summary>
    /// Data extensions for manipulation of continent objects
    /// </summary>
    public static class ContinentExtensions
    {
        /// <summary>
        /// Create a continent from the current row in the data reader
        /// </summary>
        /// <param name="rdr">The data reader</param>
        /// <returns>The current row's values as a continent object</returns>
        private static Continent ToContinent(NpgsqlDataReader rdr) =>
            new Continent(ContinentId.Parse(rdr.GetString("id")), rdr.GetString("name"));

        /// <summary>
        /// Retrieve all continents
        /// </summary>
        /// <returns>All continents</returns>
        public static async Task<IEnumerable<Continent>> AllContinents(this NpgsqlConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM continent ORDER BY name";

            using var rdr = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            var continents = new List<Continent>();
            while (await rdr.ReadAsync())
            {
                continents.Add(ToContinent(rdr));
            }

            return continents;
        }
    }
}
