using JobsJobsJobs.Shared;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JobsJobsJobs.Server.Data
{
    /// <summary>
    /// Converters used to translate between database and domain types
    /// </summary>
    public static class Converters
    {
        /// <summary>
        /// Citizen ID converter
        /// </summary>
        public static readonly ValueConverter<CitizenId, string> CitizenIdConverter =
            new(v => v.ToString(), v => CitizenId.Parse(v));

        /// <summary>
        /// Continent ID converter
        /// </summary>
        public static readonly ValueConverter<ContinentId, string> ContinentIdConverter =
            new(v => v.ToString(), v => ContinentId.Parse(v));

        /// <summary>
        /// Markdown converter
        /// </summary>
        public static readonly ValueConverter<MarkdownString, string> MarkdownStringConverter =
            new(v => v.Text, v => new MarkdownString(v));

        /// <summary>
        /// Markdown converter for possibly-null values
        /// </summary>
        public static readonly ValueConverter<MarkdownString?, string?> OptionalMarkdownStringConverter =
            new(v => v == null ? null : v.Text, v => v == null ? null : new MarkdownString(v));

        /// <summary>
        /// Skill ID converter
        /// </summary>
        public static readonly ValueConverter<SkillId, string> SkillIdConverter =
            new(v => v.ToString(), v => SkillId.Parse(v));

        /// <summary>
        /// Success ID converter
        /// </summary>
        public static readonly ValueConverter<SuccessId, string> SuccessIdConverter =
            new(v => v.ToString(), v => SuccessId.Parse(v));
    }
}
