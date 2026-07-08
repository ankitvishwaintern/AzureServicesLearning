using Controllers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AzureServicesLearning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemoryController : ControllerBase
    {
        private readonly ILogger<SampleDataController> _logger;
        private readonly TelemetryClient _telemetryClient;
        private static readonly ActivitySource ActivitySource = new("MemoryController");
        // Issue 1: Static references never get cleared by Garbage Collection.
        // Every time this endpoint is hit, memory consumption climbs permanently.
        private static readonly List<byte[]> _globalReportCache = new List<byte[]>();

        public MemoryController(ILogger<SampleDataController> logger, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
        }
        [HttpGet("generate")]
        public IActionResult GenerateBigReport()
        {
            // Fixed: Removed the intentional massive allocation (int.MaxValue) that
            // guaranteed an OutOfMemoryException. Use a safe, bounded size instead,
            // and avoid retaining references in the static cache to prevent unbounded
            // memory growth across requests.
            try
            {
                const int SafeArraySize = 1024 * 1024; // 1M ints (~4MB), safe and bounded
                int[] safeArray = new int[SafeArraySize];

                // Do not add to _globalReportCache to avoid permanent memory retention.
                // If caching is required, use a bounded/expiring cache (e.g., IMemoryCache)
                // instead of a static List<byte[]>.

                return Ok(new { Message = "Report generated successfully.", Length = safeArray.Length });
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                throw;
            }
        }
    }
        
}