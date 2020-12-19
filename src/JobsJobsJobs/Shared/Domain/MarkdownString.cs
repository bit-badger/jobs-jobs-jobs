using Markdig;

namespace JobsJobsJobs.Shared
{
    /// <summary>
    /// A string of Markdown text
    /// </summary>
    public record MarkdownString(string Text)
    {
        /// <summary>
        /// The Markdown conversion pipeline (enables all advanced features)
        /// </summary>
        private readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

        /// <summary>
        /// Convert this Markdown string to HTML
        /// </summary>
        /// <returns>This Markdown string as HTML</returns>
        public string ToHtml() => Markdown.ToHtml(Text, Pipeline);
    }
}
