using JobsJobsJobs.Shared;
using NodaTime;
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
    public static class NpgsqlExtensions
    {
        #region Data Reader

        /// <summary>
        /// Get a boolean by its name
        /// </summary>
        /// <param name="name">The name of the field to be retrieved as a boolean</param>
        /// <returns>The specified field as a boolean</returns>
        public static bool GetBoolean(this NpgsqlDataReader rdr, string name) => rdr.GetBoolean(rdr.GetOrdinal(name));

        /// <summary>
        /// Get an Instant by its name
        /// </summary>
        /// <param name="name">The name of the field to be retrieved as an Instant</param>
        /// <returns>The specified field as an Instant</returns>
        public static Instant GetInstant(this NpgsqlDataReader rdr, string name) =>
            rdr.GetFieldValue<Instant>(rdr.GetOrdinal(name));

        /// <summary>
        /// Get a 64-bit integer by its name
        /// </summary>
        /// <param name="name">The name of the field to be retrieved as a 64-bit integer</param>
        /// <returns>The specified field as a 64-bit integer</returns>
        public static long GetInt64(this NpgsqlDataReader rdr, string name) => rdr.GetInt64(rdr.GetOrdinal(name));

        /// <summary>
        /// Get a string by its name
        /// </summary>
        /// <param name="name">The name of the field to be retrieved as a string</param>
        /// <returns>The specified field as a string</returns>
        public static string GetString(this NpgsqlDataReader rdr, string name) => rdr.GetString(rdr.GetOrdinal(name));

        /// <summary>
        /// Determine if a column is null
        /// </summary>
        /// <param name="name">The name of the column to check</param>
        /// <returns>True if the column is null, false if not</returns>
        public static bool IsDBNull(this NpgsqlDataReader rdr, string name) => rdr.IsDBNull(rdr.GetOrdinal(name));

        #endregion

        #region Command

        /// <summary>
        /// Add a string parameter
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        public static void AddString(this NpgsqlCommand cmd, string name, object value) =>
            cmd.Parameters.Add(
                new NpgsqlParameter<string>($"@{name}", value is string @val ? @val : value.ToString()!));

        /// <summary>
        /// Add a boolean parameter
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        public static void AddBool(this NpgsqlCommand cmd, string name, bool value) =>
            cmd.Parameters.Add(new NpgsqlParameter<bool>($"@{name}", value));

        /// <summary>
        /// Add an Instant parameter
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        public static void AddInstant(this NpgsqlCommand cmd, string name, Instant value) =>
            cmd.Parameters.Add(new NpgsqlParameter<Instant>($"@{name}", value));

        /// <summary>
        /// Add a parameter that may be null
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        public static void AddMaybeNull(this NpgsqlCommand cmd, string name, object? value) =>
            cmd.Parameters.Add(new NpgsqlParameter($"@{name}", value == null ? DBNull.Value : value));

        #endregion
    }
}
