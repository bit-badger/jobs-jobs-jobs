using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JobsJobsJobs.Shared
{
    /// <summary>
    /// A short ID
    /// </summary>
    public record ShortId(string Id)
    {
        /// <summary>
        /// Validate the format of the short ID
        /// </summary>
        private static readonly Regex ValidShortId =
            new Regex("^[a-z0-9_-]{12}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Create a new short ID
        /// </summary>
        /// <returns>A new short ID</returns>
        public static async Task<ShortId> Create() => new ShortId(await Nanoid.Nanoid.GenerateAsync(size: 12));

        /// <summary>
        /// Try to parse a string of text into a short ID
        /// </summary>
        /// <param name="text">The text of the prospective short ID</param>
        /// <returns>The short ID</returns>
        /// <exception cref="FormatException">If the format is not valid</exception>
        public static ShortId Parse(string text)
        {
            if (text.Length == 12 && ValidShortId.IsMatch(text)) return new ShortId(text);
            throw new FormatException($"The string {text} is not a valid short ID");
        }

        public override string ToString() => Id;
    }
}
