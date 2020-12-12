using JobsJobsJobs.Shared;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Data
{
    /// <summary>
    /// Extensions to the Npgsql data reader
    /// </summary>
    public static class NpgsqlDataReaderExtensions
    {
        /// <summary>
        /// Get a string by its name
        /// </summary>
        /// <param name="name">The name of the field to be retrieved as a string</param>
        /// <returns>The specified field as a string</returns>
        public static string GetString(this NpgsqlDataReader rdr, string name) => rdr.GetString(rdr.GetOrdinal(name));

        /// <summary>
        /// Get a 64-bit integer by its name
        /// </summary>
        /// <param name="name">The name of the field to be retrieved as a 64-bit integer</param>
        /// <returns>The specified field as a 64-bit integer</returns>
        public static long GetInt64(this NpgsqlDataReader rdr, string name) => rdr.GetInt64(rdr.GetOrdinal(name));

        /// <summary>
        /// Get milliseconds by its name
        /// </summary>
        /// <param name="name">The name of the field to be retrieved as milliseconds</param>
        /// <returns>The specified field as milliseconds</returns>
        public static Milliseconds GetMilliseconds(this NpgsqlDataReader rdr, string name) =>
            new Milliseconds(rdr.GetInt64(name));
    }
}
