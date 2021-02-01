using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Shared
{
    public partial class Loading : ComponentBase
    {
        /// <summary>
        /// The delegate to call to load the data for this page
        /// </summary>
        [Parameter]
        public EventCallback<ICollection<string>> OnLoad { get; set; }

        /// <summary>
        /// The message to display when the page is loading (optional)
        /// </summary>
        [Parameter]
        public MarkupString Message { get; set; } = new MarkupString("Loading&hellip;");

        /// <summary>
        /// The content to be displayed once the data has been loaded
        /// </summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; } = default!;

        /// <summary>
        /// Error messages that may arise from the data loading delegate
        /// </summary>
        private ICollection<string> ErrorMessages { get; set; } = new List<string>();

        /// <summary>
        /// Whether we are currently loading data
        /// </summary>
        private bool IsLoading { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            if (OnLoad.HasDelegate)
            {
                try
                {
                    await OnLoad.InvokeAsync(ErrorMessages);
                }
                finally
                {
                    IsLoading = false;
                }
            }
            else
            {
                IsLoading = false;
            }
        }
    }
}
