using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Pages.Citizen
{
    /// <summary>
    /// Profile edit page (called EditProfile so as not to create naming conflicts)
    /// </summary>
    public partial class EditProfile : ComponentBase
    {
        /// <summary>
        /// Counter for IDs when "Add a Skill" button is clicked
        /// </summary>
        private int _newSkillCounter = 0;

        /// <summary>
        /// Whether the citizen is seeking employment at the time the profile is loaded (used to show success story
        /// link)
        /// </summary>
        private bool IsSeeking { get; set; } = false;

        /// <summary>
        /// The form for this page
        /// </summary>
        private ProfileForm ProfileForm { get; set; } = new ProfileForm();

        /// <summary>
        /// All continents
        /// </summary>
        private IEnumerable<Continent> Continents { get; set; } = Enumerable.Empty<Continent>();

        /// <summary>
        /// Whether this is a new profile or not
        /// </summary>
        private bool IsNew { get; set; } = false;

        /// <summary>
        /// Set up the data needed to add or edit the user's profile
        /// </summary>
        /// <param name="errors">The collection where errors can be reported</param>
        public async Task SetUpProfile(ICollection<string> errors)
        {
            ServerApi.SetJwt(http, state);
            var continentTask = state.GetContinents(http);
            var profileTask = ServerApi.RetrieveProfile(http, state);

            await Task.WhenAll(continentTask, profileTask);

            Continents = continentTask.Result;

            if (profileTask.Result.IsOk)
            {
                if (profileTask.Result.Ok == null)
                {
                    ProfileForm = new ProfileForm();
                    IsNew = true;
                }
                else
                {
                    ProfileForm = ProfileForm.FromProfile(profileTask.Result.Ok);
                    IsSeeking = profileTask.Result.Ok.SeekingEmployment;
                }
                if (ProfileForm.Skills.Count == 0) AddNewSkill();
            }
            else
            {
                errors.Add(profileTask.Result.Error);
            }
        }

        /// <summary>
        /// Add a new skill to the form
        /// </summary>
        private void AddNewSkill() =>
            ProfileForm.Skills.Add(new SkillForm { Id = $"new{_newSkillCounter++}" });

        /// <summary>
        /// Remove the skill for the given ID
        /// </summary>
        /// <param name="skillId">The ID of the skill to remove</param>
        private void RemoveSkill(string skillId) =>
            ProfileForm.Skills.Remove(ProfileForm.Skills.First(s => s.Id == skillId));

        /// <summary>
        /// Save changes to the current profile
        /// </summary>
        public async Task SaveProfile()
        {
            // Remove any skills left blank
            var blankSkills = ProfileForm.Skills
                .Where(s => string.IsNullOrEmpty(s.Description) && string.IsNullOrEmpty(s.Notes))
                .ToList();
            foreach (var blankSkill in blankSkills) ProfileForm.Skills.Remove(blankSkill);

            var res = await http.PostAsJsonAsync("/api/profile/save", ProfileForm);
            if (res.IsSuccessStatusCode)
            {
                toast.ShowSuccess("Profile Saved Successfully");
                nav.NavigateTo($"/profile/view/{state.User!.Id}");
            }
            else
            {
                var error = await res.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(error)) error = $"- {error}";
                toast.ShowError($"{(int)res.StatusCode} {error}");
            }
        }
    }
}
