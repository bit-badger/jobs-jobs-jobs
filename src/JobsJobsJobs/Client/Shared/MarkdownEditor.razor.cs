using Markdig;
using Microsoft.AspNetCore.Components;

namespace JobsJobsJobs.Client.Shared
{
    /// <summary>
    /// Code-behind for the Markdown Editor component
    /// </summary>
    public partial class MarkdownEditor : ComponentBase
    {
        /// <summary>
        /// Pipeline with most extensions enabled
        /// </summary>
        private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
            .UseSmartyPants().UseAdvancedExtensions().Build();

        /// <summary>
        /// Whether the preview should be shown
        /// </summary>
        private bool _showPreview = false;

        /// <summary>
        /// Backing field for the plain-text representation of this document
        /// </summary>
        private string _text = "";

        /// <summary>
        /// The plain-text (Markdown source)
        /// </summary>
        private string PlainText {
            get => _text;
            set
            {
                _text = value;
                TextChanged.InvokeAsync(_text).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// The rendered HTML
        /// </summary>
        private MarkupString PreviewText => (MarkupString)Markdown.ToHtml(_text, _pipeline);

        /// <summary>
        /// CSS class for the "Markdown" tab (active if preview not shown)
        /// </summary>
        private string MarkdownClass => _showPreview ? "" : "active";

        /// <summary>
        /// CSS class for the "Preview" tab (active if preview shown)
        /// </summary>
        private string PreviewClass => _showPreview ? "active" : "";

        /// <summary>
        /// The ID used for the textarea; allows for a label to be assigned
        /// </summary>
        [Parameter]
        public string Id { get; set; } = "";

        /// <summary>
        /// The text value of the Markdown
        /// </summary>
        [Parameter]
        public string Text { get; set; } = "";

        /// <summary>
        /// Event fired when the text is changed
        /// </summary>
        [Parameter]
        public EventCallback<string> TextChanged { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _text = Text;
        }

        /// <summary>
        /// Show the Markdown editor
        /// </summary>
        private void ShowMarkdown() => _showPreview = false;

        /// <summary>
        /// Show the Markdown preview
        /// </summary>
        private void ShowPreview() => _showPreview = true;
    }
}
