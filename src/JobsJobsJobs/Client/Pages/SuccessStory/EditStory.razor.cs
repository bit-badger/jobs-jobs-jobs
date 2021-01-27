using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Pages.SuccessStory
{
    public partial class EditStory : ComponentBase
    {
        /// <summary>
        /// The ID of the success story being edited
        /// </summary>
        [Parameter]
        public string? Id { get; set; }

        /// <summary>
        /// The page title / header
        /// </summary>
        public string Title => IsNew ? "Tell Your Success Story" : "Edit Success Story";

        /// <summary>
        /// The form with information for the success story
        /// </summary>
        private StoryForm Form { get; set; } = new StoryForm();

        /// <summary>
        /// Convenience property for showing new
        /// </summary>
        private bool IsNew => Form.Id == "new";

        /// <summary>
        /// Retrieve the story
        /// </summary>
        /// <param name="errors">A collection to use in reporting errors that may occur</param>
        public async Task RetrieveStory(ICollection<string> errors)
        {
            if (Id != null)
            {
                ServerApi.SetJwt(http, state);
                var story = await ServerApi.RetrieveOne<Success>(http, $"success/{Id}");
                if (story.IsOk && story.Ok != null)
                {
                    Form = new StoryForm
                    {
                        Id = story.Ok.Id.ToString(),
                        FromHere = story.Ok.FromHere,
                        Story = story.Ok.Story?.Text ?? ""
                    };
                }
                else if (story.IsOk)
                {
                    errors.Add($"The success story {Id} does not exist");
                }
                else
                {
                    errors.Add(story.Error);
                }
            }
        }

        /// <summary>
        /// Save the success story
        /// </summary>
        private async Task SaveStory()
        {
            ServerApi.SetJwt(http, state);
            var res = await http.PostAsJsonAsync("/api/success/save", Form);

            if (res.IsSuccessStatusCode)
            {
                if (IsNew)
                {
                    res = await http.PatchAsync("/api/profile/employment-found", new StringContent(""));
                    if (res.IsSuccessStatusCode)
                    {
                        SaveSuccessful();
                    }
                    else
                    {
                        await SaveFailed(res);
                    }
                }
                else
                {
                    SaveSuccessful();
                }
            }
            else
            {
                await SaveFailed(res);
            }
        }

        /// <summary>
        /// Handle success notifications if saving succeeded
        /// </summary>
        private void SaveSuccessful()
        {
            toast.ShowSuccess("Story Saved Successfully");
            nav.NavigateTo("/success-story/list");
        }

        /// <summary>
        /// Handle failure notifications is saving was not successful
        /// </summary>
        /// <param name="res">The HTTP response</param>
        private async Task SaveFailed(HttpResponseMessage res)
        {
            var error = await res.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(error)) error = $"- {error}";
            toast.ShowError($"{(int)res.StatusCode} {error}");
        }
    }
}
