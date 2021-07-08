using JobsJobsJobs.Server.Data;
using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Areas.Api.Controllers
{
    /// <summary>
    /// API controller for success stories
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class SuccessController : Controller
    {
        /// <summary>
        /// The data context
        /// </summary>
        private readonly JobsDbContext _db;

        /// <summary>
        /// The NodaTime clock instance
        /// </summary>
        private readonly IClock _clock;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">The data context to use for this request</param>
        /// <param name="clock">The clock instance to use for this request</param>
        public SuccessController(JobsDbContext db, IClock clock)
        {
            _db = db;
            _clock = clock;
        }

        /// <summary>
        /// The current citizen ID
        /// </summary>
        private CitizenId CurrentCitizenId => CitizenId.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet("{id}")]
        public async Task<IActionResult> Retrieve(string id) =>
            Ok(await _db.FindSuccessById(SuccessId.Parse(id)));

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] StoryForm form)
        {
            if (form.Id == "new")
            {
                var story = new Success(await SuccessId.Create(), CurrentCitizenId, _clock.GetCurrentInstant(),
                    form.FromHere, // "profile",
                    string.IsNullOrWhiteSpace(form.Story) ? null : new MarkdownString(form.Story));
                await _db.AddAsync(story);
            }
            else
            {
                var story = await _db.FindSuccessById(SuccessId.Parse(form.Id));
                if (story == null) return NotFound();
                var updated = story with
                {
                    FromHere = form.FromHere,
                    Story = string.IsNullOrWhiteSpace(form.Story) ? null : new MarkdownString(form.Story)
                };
                _db.Update(updated);
            }
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("list")]
        public async Task<IActionResult> List() =>
            Ok(await _db.AllStories());
    }
}
