using Blazored.Toast.Services;
using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        /// A flag that indicates all the required API calls have completed, and the form is ready to be displayed
        /// </summary>
        private bool AllLoaded { get; set; } = false;

        /// <summary>
        /// The form for this page
        /// </summary>
        private ProfileForm ProfileForm { get; set; } = new ProfileForm();

        /// <summary>
        /// All continents
        /// </summary>
        private IEnumerable<Continent> Continents { get; set; } = Enumerable.Empty<Continent>();

        /// <summary>
        /// Error messages from API access
        /// </summary>
        private IList<string> ErrorMessages { get; } = new List<string>();

        /// <summary>
        /// HTTP client instance to use for API access
        /// </summary>
        [Inject]
        private HttpClient Http { get; set; } = default!;

        /// <summary>
        /// Application state
        /// </summary>
        [Inject]
        private AppState State { get; set; } = default!;

        /// <summary>
        /// Toast service
        /// </summary>
        [Inject]
        private IToastService Toasts { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            ServerApi.SetJwt(Http, State);
            var continentTask = ServerApi.RetrieveMany<Continent>(Http, "continent/all");
            var profileTask = ServerApi.RetrieveProfile(Http, State);
            var skillTask = ServerApi.RetrieveMany<Skill>(Http, "profile/skills");

            await Task.WhenAll(continentTask, profileTask, skillTask);

            if (continentTask.Result.IsOk)
            {
                Continents = continentTask.Result.Ok;
            }
            else
            {
                ErrorMessages.Add(continentTask.Result.Error);
            }

            if (profileTask.Result.IsOk)
            {
                ProfileForm = (profileTask.Result.Ok == null)
                    ? new ProfileForm()
                    : ProfileForm.FromProfile(profileTask.Result.Ok);
            }
            else
            {
                ErrorMessages.Add(profileTask.Result.Error);
            }

            if (skillTask.Result.IsOk)
            {
                foreach (var skill in skillTask.Result.Ok)
                {
                    ProfileForm.Skills.Add(new SkillForm
                    {
                        Id = skill.Id.ToString(),
                        Description = skill.Description,
                        Notes = skill.Notes ?? ""
                    });
                }
                if (ProfileForm.Skills.Count == 0) AddNewSkill();
            }
            else
            {
                ErrorMessages.Add(skillTask.Result.Error);
            }

            AllLoaded = true;
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

            var res = await Http.PostAsJsonAsync("/api/profile/save", ProfileForm);
            if (res.IsSuccessStatusCode)
            {
                Toasts.ShowSuccess("Profile Saved Successfully");
            }
            else
            {
                var error = await res.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(error)) error = $"- {error}";
                Toasts.ShowError($"{(int)res.StatusCode} {error}");
            }
        }

    }
}
