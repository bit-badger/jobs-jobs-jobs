using JobsJobsJobs.Server.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server.Areas.Api.Controllers
{
    /// <summary>
    /// API endpoint for continent information
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ContinentController : ControllerBase
    {
        /// <summary>
        /// The data context to use for this request
        /// </summary>
        private readonly JobsDbContext _db;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">The data context to use for this request</param>
        public ContinentController(JobsDbContext db)
        {
            _db = db;
        }

        [HttpGet("all")]
        public async Task<IActionResult> All() =>
            Ok(await _db.AllContinents());
    }
}
