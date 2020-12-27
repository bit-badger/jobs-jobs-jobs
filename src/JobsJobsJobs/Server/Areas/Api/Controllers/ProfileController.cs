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
    /// <summary>
    /// API controller for employment profile information
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        /// <summary>
        /// The database connection
        /// </summary>
        private readonly NpgsqlConnection _db;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">The database connection to use for this request</param>
        public ProfileController(NpgsqlConnection db)
        {
            _db = db;
        }

        [Authorize]
        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            await _db.OpenAsync();
            var profile = await _db.FindProfileByCitizen(
                CitizenId.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value));
            return profile == null ? NoContent() : Ok(profile);
        }
    }
}
