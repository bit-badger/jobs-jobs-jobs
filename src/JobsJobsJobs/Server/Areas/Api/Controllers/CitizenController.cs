using JobsJobsJobs.Server.Data;
using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NodaTime;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Areas.Api.Controllers
{
    /// <summary>
    /// API controller for citizen information
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CitizenController : ControllerBase
    {
        /// <summary>
        /// Authorization configuration section
        /// </summary>
        private readonly IConfigurationSection _config;

        /// <summary>
        /// NodaTime clock
        /// </summary>
        private readonly IClock _clock;

        /// <summary>
        /// The data context to use for this request
        /// </summary>
        private readonly JobsDbContext _db;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The authorization configuration section</param>
        /// <param name="clock">The NodaTime clock instance</param>
        /// <param name="db">The data context to use for this request</param>
        public CitizenController(IConfiguration config, IClock clock, JobsDbContext db)
        {
            _config = config.GetSection("Auth");
            _clock = clock;
            _db = db;
        }

        [HttpGet("log-on/{authCode}")]
        public async Task<IActionResult> LogOn([FromRoute] string authCode)
        {
            // Step 1 - Verify with Mastodon
            var accountResult = await Auth.VerifyWithMastodon(authCode, _config);

            if (accountResult.IsError) return BadRequest(accountResult.Error);

            // Step 2 - Find / establish Jobs, Jobs, Jobs account
            var account = accountResult.Ok;
            var now = _clock.GetCurrentInstant();

            var citizen = await _db.FindCitizenByNAUser(account.Username);
            if (citizen == null)
            {
                citizen = new Citizen(await CitizenId.Create(), account.Username, account.DisplayName, account.Url,
                    now, now);
                await _db.AddCitizen(citizen);
            }
            else
            {
                citizen = citizen with
                {
                    DisplayName = account.DisplayName,
                    LastSeenOn = now
                };
                _db.UpdateCitizen(citizen);
            }
            await _db.SaveChangesAsync();

            // Step 3 - Generate JWT
            var jwt = Auth.CreateJwt(citizen, _config);

            return new JsonResult(new LogOnSuccess(jwt, citizen.Id.ToString(), citizen.DisplayName));
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetCitizenById([FromRoute] string id)
        {
            var citizen = await _db.FindCitizenById(CitizenId.Parse(id));
            return citizen == null ? NotFound() : Ok(citizen);
        }
    }
}
