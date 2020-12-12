using JobsJobsJobs.Server.Data;
using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Areas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitizenController : ControllerBase
    {
        private readonly IConfigurationSection _config;

        private readonly NpgsqlConnection _db;

        public CitizenController(IConfiguration config, NpgsqlConnection db)
        {
            _config = config.GetSection("Auth");
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
            var now = Milliseconds.Now();

            await _db.OpenAsync();
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
                    LastSeenOn = Milliseconds.Now()
                };
                await _db.UpdateCitizenOnLogOn(citizen);
            }

            // Step 3 - Generate JWT
            var jwt = Auth.CreateJwt(citizen, _config);

            return new JsonResult(new LogOnSuccess(jwt, citizen.Id.ToString(), citizen.DisplayName));
        }
    }
}
