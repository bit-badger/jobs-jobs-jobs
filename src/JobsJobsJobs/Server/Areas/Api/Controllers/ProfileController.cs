using JobsJobsJobs.Server.Data;
using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Areas.Api.Controllers
{
    /// <summary>
    /// API controller for employment profile information
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ProfileController : ControllerBase
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
        public ProfileController(JobsDbContext db, IClock clock)
        {
            _db = db;
            _clock = clock;
        }

        /// <summary>
        /// The current citizen ID
        /// </summary>
        private CitizenId CurrentCitizenId => CitizenId.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // This returns 204 to indicate that there is no profile data for the current citizen (if, of course, that is
        // the case). The version where an ID is specified returns 404, which is an error condition, as it should not
        // occur unless someone is messing with a URL.
        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            var profile = await _db.FindProfileByCitizen(CurrentCitizenId);
            return profile == null ? NoContent() : Ok(profile);
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save(ProfileForm form)
        {
            // Profile
            var existing = await _db.FindProfileByCitizen(CurrentCitizenId);
            var profile = existing == null
                ? new Profile(CurrentCitizenId, form.IsSeekingEmployment, form.IsPublic,
                    ContinentId.Parse(form.ContinentId), form.Region, form.RemoteWork, form.FullTime,
                    new MarkdownString(form.Biography), _clock.GetCurrentInstant(),
                    string.IsNullOrEmpty(form.Experience) ? null : new MarkdownString(form.Experience))
                : existing with
                {
                    SeekingEmployment = form.IsSeekingEmployment,
                    IsPublic = form.IsPublic,
                    ContinentId = ContinentId.Parse(form.ContinentId),
                    Region = form.Region,
                    RemoteWork = form.RemoteWork,
                    FullTime = form.FullTime,
                    Biography = new MarkdownString(form.Biography),
                    LastUpdatedOn = _clock.GetCurrentInstant(),
                    Experience = string.IsNullOrEmpty(form.Experience) ? null : new MarkdownString(form.Experience)
                };
            await _db.SaveProfile(profile);

            // Skills
            var skills = new List<Skill>();
            foreach (var skill in form.Skills) {
                skills.Add(new Skill(skill.Id.StartsWith("new") ? await SkillId.Create() : SkillId.Parse(skill.Id),
                    CurrentCitizenId, skill.Description, string.IsNullOrEmpty(skill.Notes) ? null : skill.Notes));
            }
            
            foreach (var skill in skills) await _db.SaveSkill(skill);
            await _db.DeleteMissingSkills(CurrentCitizenId, skills.Select(s => s.Id));

            // Real Name
            _db.Update((await _db.FindCitizenById(CurrentCitizenId))!
                with { RealName = string.IsNullOrWhiteSpace(form.RealName) ? null : form.RealName });

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetProfileCount() =>
            Ok(new Count(await _db.CountProfiles()));

        [HttpGet("skill-count")]
        public async Task<IActionResult> GetSkillCount() =>
            Ok(new Count(await _db.CountSkillsByCitizen(CurrentCitizenId)));

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetProfileForCitizen([FromRoute] string id)
        {
            var profile = await _db.FindProfileByCitizen(CitizenId.Parse(id));
            return profile == null ? NotFound() : Ok(profile);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] ProfileSearch search) =>
            Ok(await _db.SearchProfiles(search));

        [HttpGet("public-search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchPublic([FromQuery] PublicSearch search) =>
            Ok(await _db.SearchPublicProfiles(search));

        [HttpPatch("employment-found")]
        public async Task<IActionResult> EmploymentFound()
        {
            var profile = await _db.FindProfileByCitizen(CurrentCitizenId);
            if (profile == null) return NotFound();

            var updated = profile with { SeekingEmployment = false };
            _db.Update(updated);

            await _db.SaveChangesAsync();

            return Ok();
        }
        
        [HttpDelete("")]
        public async Task<IActionResult> Remove()
        {
            await _db.DeleteProfileByCitizen(CurrentCitizenId);
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
