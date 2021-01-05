using JobsJobsJobs.Server.Data;
using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime;
using Npgsql;
using System;
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
        /// The database connection
        /// </summary>
        private readonly NpgsqlConnection _db;

        /// <summary>
        /// The NodaTime clock instance
        /// </summary>
        private readonly IClock _clock;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">The database connection to use for this request</param>
        public ProfileController(NpgsqlConnection db, IClock clock)
        {
            _db = db;
            _clock = clock;
        }

        /// <summary>
        /// The current citizen ID
        /// </summary>
        private CitizenId CurrentCitizenId => CitizenId.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            await _db.OpenAsync();
            var profile = await _db.FindProfileByCitizen(CurrentCitizenId);
            return profile == null ? NoContent() : Ok(profile);
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save(ProfileForm form)
        {
            await _db.OpenAsync();
            var txn = await _db.BeginTransactionAsync();

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

            await txn.CommitAsync();
            return Ok();
        }

        [HttpGet("skills")]
        public async Task<IActionResult> GetSkills()
        {
            await _db.OpenAsync();
            return Ok(await _db.FindSkillsByCitizen(CurrentCitizenId));
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetProfileCount()
        {
            await _db.OpenAsync();
            return Ok(new Count(await _db.CountProfiles()));
        }

        [HttpGet("skill-count")]
        public async Task<IActionResult> GetSkillCount()
        {
            await _db.OpenAsync();
            return Ok(new Count(await _db.CountSkills(CurrentCitizenId)));
        }
    }
}
