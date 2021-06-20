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

    /// <summary>
    /// The ID of a user (a citizen of Gitmo Nation)
    /// </summary>
    public record CitizenId(ShortId Id)
    {
        /// <summary>
        /// Create a new citizen ID
        /// </summary>
        /// <returns>A new citizen ID</returns>
        public static async Task<CitizenId> Create() => new CitizenId(await ShortId.Create());

        /// <summary>
        /// Attempt to create a citizen ID from a string
        /// </summary>
        /// <param name="id">The prospective ID</param>
        /// <returns>The citizen ID</returns>
        /// <exception cref="System.FormatException">If the string is not a valid citizen ID</exception>
        public static CitizenId Parse(string id) => new(ShortId.Parse(id));

        public override string ToString() => Id.ToString();
    }

    /// <summary>
    /// The ID of a continent
    /// </summary>
    public record ContinentId(ShortId Id)
    {
        /// <summary>
        /// Create a new continent ID
        /// </summary>
        /// <returns>A new continent ID</returns>
        public static async Task<ContinentId> Create() => new ContinentId(await ShortId.Create());

        /// <summary>
        /// Attempt to create a continent ID from a string
        /// </summary>
        /// <param name="id">The prospective ID</param>
        /// <returns>The continent ID</returns>
        /// <exception cref="System.FormatException">If the string is not a valid continent ID</exception>
        public static ContinentId Parse(string id) => new(ShortId.Parse(id));

        public override string ToString() => Id.ToString();
    }

    /// <summary>
    /// The ID of a job listing
    /// </summary>
    public record ListingId(ShortId Id)
    {
        /// <summary>
        /// Create a new job listing ID
        /// </summary>
        /// <returns>A new job listing ID</returns>
        public static async Task<ListingId> Create() => new ListingId(await ShortId.Create());

        /// <summary>
        /// Attempt to create a job listing ID from a string
        /// </summary>
        /// <param name="id">The prospective ID</param>
        /// <returns>The job listing ID</returns>
        /// <exception cref="System.FormatException">If the string is not a valid job listing ID</exception>
        public static ListingId Parse(string id) => new(ShortId.Parse(id));

        public override string ToString() => Id.ToString();
    }

    /// <summary>
    /// The ID of a skill
    /// </summary>
    public record SkillId(ShortId Id)
    {
        /// <summary>
        /// Create a new skill ID
        /// </summary>
        /// <returns>A new skill ID</returns>
        public static async Task<SkillId> Create() => new SkillId(await ShortId.Create());

        /// <summary>
        /// Attempt to create a skill ID from a string
        /// </summary>
        /// <param name="id">The prospective ID</param>
        /// <returns>The skill ID</returns>
        /// <exception cref="System.FormatException">If the string is not a valid skill ID</exception>
        public static SkillId Parse(string id) => new(ShortId.Parse(id));

        public override string ToString() => Id.ToString();
    }

    /// <summary>
    /// The ID of a success report
    /// </summary>
    public record SuccessId(ShortId Id)
    {
        /// <summary>
        /// Create a new success report ID
        /// </summary>
        /// <returns>A new success report ID</returns>
        public static async Task<SuccessId> Create() => new SuccessId(await ShortId.Create());

        /// <summary>
        /// Attempt to create a success report ID from a string
        /// </summary>
        /// <param name="id">The prospective ID</param>
        /// <returns>The success report ID</returns>
        /// <exception cref="System.FormatException">If the string is not a valid success report ID</exception>
        public static SuccessId Parse(string id) => new(ShortId.Parse(id));

        public override string ToString() => Id.ToString();
    }
}
