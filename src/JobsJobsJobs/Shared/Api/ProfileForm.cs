﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace JobsJobsJobs.Shared.Api
{
    /// <summary>
    /// The data required to update a profile
    /// </summary>
    public class ProfileForm
    {
        /// <summary>
        /// Whether the citizen to whom this profile belongs is actively seeking employment
        /// </summary>
        public bool IsSeekingEmployment { get; set; }

        /// <summary>
        /// Whether this profile should appear in the public search
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// The user's real name
        /// </summary>
        [StringLength(255)]
        public string RealName { get; set; } = "";

        /// <summary>
        /// The ID of the continent on which the citizen is located
        /// </summary>
        [Required]
        [StringLength(12, MinimumLength = 1)]
        [Display(Name = "Continent")]
        public string ContinentId { get; set; } = "";

        /// <summary>
        /// The area within that continent where the citizen is located
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Region { get; set; } = "";

        /// <summary>
        /// If the citizen is available for remote work
        /// </summary>
        public bool RemoteWork { get; set; }

        /// <summary>
        /// If the citizen is seeking full-time employment
        /// </summary>
        public bool FullTime { get; set; }

        /// <summary>
        /// The user's professional biography
        /// </summary>
        [Required]
        public string Biography { get; set; } = "";

        /// <summary>
        /// The user's past experience
        /// </summary>
        public string Experience { get; set; } = "";
        
        /// <summary>
        /// The skills for the user
        /// </summary>
        public ICollection<SkillForm> Skills { get; set; } = new List<SkillForm>();

        /// <summary>
        /// Create an instance of this form from the given profile
        /// </summary>
        /// <param name="profile">The profile off which this form will be based</param>
        /// <returns>The profile form, popluated with values from the given profile</returns>
        public static ProfileForm FromProfile(Profile profile) =>
            new ProfileForm
            {
                IsSeekingEmployment = profile.SeekingEmployment,
                IsPublic = profile.IsPublic,
                ContinentId = profile.ContinentId.ToString(),
                Region = profile.Region,
                RemoteWork = profile.RemoteWork,
                FullTime = profile.FullTime,
                Biography = profile.Biography.Text,
                Experience = profile.Experience?.Text ?? "",
                Skills = profile.Skills.Select(s => new SkillForm
                {
                    Id = s.Id.ToString(),
                    Description = s.Description,
                    Notes = s.Notes
                }).ToList()
            };
    }
}
