using JobsJobsJobs.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Areas.Api.Controllers
{
    /// <summary>
    /// API endpoint for continent information
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ContinentController : ControllerBase
    {
        /// <summary>
        /// The database connection to use for this request
        /// </summary>
        private readonly NpgsqlConnection _db;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">The database connection to use for this request</param>
        public ContinentController(NpgsqlConnection db)
        {
            _db = db;
        }

        [HttpGet("all")]
        public async Task<IActionResult> All()
        {
            await _db.OpenAsync();
            return Ok(await _db.AllContinents());
        }
    }
}
