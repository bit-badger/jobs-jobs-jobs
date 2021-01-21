using Microsoft.AspNetCore.Components;
using NodaTime;
using NodaTime.Text;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Shared
{
    public partial class FullDate : ComponentBase
    {
        /// <summary>
        /// The pattern with which dates will be formatted
        /// </summary>
        private static readonly ZonedDateTimePattern Pattern =
            ZonedDateTimePattern.CreateWithCurrentCulture("ld<MMMM d, yyyy>", DateTimeZoneProviders.Tzdb);

        /// <summary>
        /// The date to be formatted
        /// </summary>
        [Parameter]
        public Instant TheDate { get; set; }

        /// <summary>
        /// The formatted date
        /// </summary>
        private string Translated { get; set; } = "";

        protected override async Task OnInitializedAsync() =>
            Translated = Pattern.Format(TheDate.InZone(await state.GetTimeZone(js)));
    }
}
