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
            // Fixed: previously allocated an array of int.MaxValue elements (~8.5GB),
            // which reliably caused an OutOfMemoryException and crashed the process.
            // Replaced with a safe, bounded allocation size to avoid unbounded memory usage.
            const int SafeArraySize = 1_000_000; // ~4MB, safe bounded allocation
            try
            {
                int[] boundedArray = new int[SafeArraySize];

                // Avoid unbounded growth: do not add to the static cache to prevent
                // permanent memory retention across requests.
                return Ok(new { Message = "Report generated safely.", Length = boundedArray.Length });
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                throw;
            }
        }
    }
        
}