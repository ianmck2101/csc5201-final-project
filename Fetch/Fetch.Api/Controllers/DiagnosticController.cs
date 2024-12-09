using Fetch.Api.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Fetch.Api.Controllers
{
    public class DiagnosticController : Controller
    {
        [Route("admin/stats")]
        [HttpGet]
        public IActionResult GetUsageStats()
        {
            var stats = UsageMiddleware.GetStats();
            return Ok(stats);
        }
    }
}
