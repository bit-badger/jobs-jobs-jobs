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

        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            await _db.OpenAsync();
            var profile = await _db.FindProfileByCitizen(
                CitizenId.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value));
            return profile == null ? NoContent() : Ok(profile);
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] ProfileForm form)
        {
            var citizenId = CitizenId.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _db.OpenAsync();
            var existing = await _db.FindProfileByCitizen(citizenId);
            var profile = existing == null
                ? new Profile(citizenId, form.IsSeekingEmployment, form.IsPublic, ContinentId.Parse(form.ContinentId),
                    form.Region, form.RemoteWork, form.FullTime, new MarkdownString(form.Biography),
                    _clock.GetCurrentInstant(),
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
            return Ok();
        }
    }
}
