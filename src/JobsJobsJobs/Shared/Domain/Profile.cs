﻿using NodaTime;
using System.Collections.Generic;

namespace JobsJobsJobs.Shared
{
    /// <summary>
    /// A job seeker profile
    /// </summary>
    public record Profile(
        CitizenId Id,
        bool SeekingEmployment,
        bool IsPublic,
        ContinentId ContinentId,
        string Region,
        bool RemoteWork,
        bool FullTime,
        MarkdownString Biography,
        Instant LastUpdatedOn,
        MarkdownString? Experience)
    {
        /// <summary>
        /// Navigation property for continent
        /// </summary>
        public Continent? Continent { get; set; }

        /// <summary>
        /// Convenience property for skills associated with a profile
        /// </summary>
        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    }
}
