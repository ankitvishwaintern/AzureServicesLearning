using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureServicesLearning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemoryController : ControllerBase
    {
        // Issue 1: Static references never get cleared by Garbage Collection.
        // Every time this endpoint is hit, memory consumption climbs permanently.
        private static readonly List<byte[]> _globalReportCache = new List<byte[]>();

        [HttpGet("generate")]
        public IActionResult GenerateBigReport()
        {
            // Fixed: Avoid allocating an unrealistic, crash-inducing array size.
            // Use a safe, bounded size instead of int.MaxValue to prevent OutOfMemoryException.
            const int SafeArraySize = 1_000_000; // ~4MB, safe and reasonable for a report simulation
            int[] safeArray = new int[SafeArraySize];

            // Avoid unbounded growth of the static cache; cap its size to prevent permanent memory retention.
            const int MaxCachedReports = 10;
            if (_globalReportCache.Count >= MaxCachedReports)
            {
                _globalReportCache.RemoveAt(0);
            }
            _globalReportCache.Add(new byte[1024]); // small fixed-size sample instead of unbounded growth

            return Ok(new { Message = "Report generated safely.", Length = safeArray.Length });
        }
    }
        
}