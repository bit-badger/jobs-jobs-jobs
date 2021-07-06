using NodaTime;
using System;

namespace JobsJobsJobs.Shared
{
    /// <summary>
    /// A job listing
    /// </summary>
    public record Listing(
        ListingId Id,
        CitizenId CitizenId,
        Instant CreatedOn,
        string Title,
        ContinentId ContinentId,
        string Region,
        bool RemoteWork,
        bool IsExpired,
        Instant UpdatedOn,
        MarkdownString Text,
        LocalDate? NeededBy,
        bool? WasFilledHere)
    {
        /// <summary>
        /// Navigation property for the citizen who created the job listing
        /// </summary>
        public Citizen? Citizen { get; set; }

        /// <summary>
        /// Navigation property for the continent
        /// </summary>
        public Continent? Continent { get; set; }
    }
}
