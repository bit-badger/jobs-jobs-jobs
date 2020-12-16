using JobsJobsJobs.Server.Data;
using JobsJobsJobs.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Areas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        /// <summary>
        /// The database connection
        /// </summary>
        private readonly NpgsqlConnection db;

        public ProfileController(NpgsqlConnection dbConn)
        {
            db = dbConn;
        }

        [Authorize]
        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            await db.OpenAsync();
            var profile = await db.FindProfileByCitizen(
                CitizenId.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value));
            return profile == null ? NoContent() : Ok(profile);
        }
    }
}
