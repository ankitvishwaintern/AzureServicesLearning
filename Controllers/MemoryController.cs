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
        // Issue 1 fixed: Removed static unbounded cache that permanently retained memory across requests.
        // If caching is required, use a bounded, expiring cache (e.g., MemoryCache) instead of a static List<byte[]>.

        public MemoryController(ILogger<SampleDataController> logger, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
        }
        [HttpGet("generate")]
        [HttpGet("generate")]
        public IActionResult GenerateBigReport()
        {
            // Fixed: Avoid allocating an unrealistically large array (int.MaxValue elements ~8.5GB)
            // which always throws OutOfMemoryException and crashes the process.
            // Use a safe, bounded size instead, and handle allocation failures gracefully.
            const int SafeArraySize = 10_000_000; // ~40 MB, a reasonable bounded allocation
            try
            {
                int[] safeArray = new int[SafeArraySize];

                return Ok(new { Message = "Report generated successfully.", Length = safeArray.Length });
            }
            catch (OutOfMemoryException ex)
            {
                _telemetryClient.TrackException(ex);
                return StatusCode(StatusCodes.Status507InsufficientStorage, new { Message = "Insufficient memory to generate report." });
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                throw;
            }
        }
        
}