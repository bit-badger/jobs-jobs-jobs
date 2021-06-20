using JobsJobsJobs.Server.Data;
using JobsJobsJobs.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Areas.Api.Controllers
{
    /// <summary>
    /// API controller for job listings
    /// </summary>
    [Route("api/listings")]
    [Authorize]
    [ApiController]
    public class ListingController : ControllerBase
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
        public ListingController(JobsDbContext db, IClock clock)
        {
            _db = db;
            _clock = clock;
        }

        /// <summary>
        /// The current citizen ID
        /// </summary>
        private CitizenId CurrentCitizenId => CitizenId.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet("mine")]
        public async Task<IActionResult> Mine() =>
            Ok(await _db.FindListingsByCitizen(CurrentCitizenId));
    }
}
