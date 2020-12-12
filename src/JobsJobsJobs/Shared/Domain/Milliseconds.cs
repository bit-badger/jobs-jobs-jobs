using System;

namespace JobsJobsJobs.Shared
{
    /// <summary>
    /// Milliseconds past the epoch (JavaScript's date storage format)
    /// </summary>
    public record Milliseconds(long Millis)
    {
        /// <summary>
        /// Get the milliseconds value for now
        /// </summary>
        /// <returns>A new milliseconds from the time now</returns>
        public static Milliseconds Now() =>
            new Milliseconds(
                (DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks) / 10000L);
    }
}
