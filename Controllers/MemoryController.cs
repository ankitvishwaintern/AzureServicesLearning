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
                // Force an instant crash on the very first hit.
                // int.MaxValue attempts to create an array with 2,147,483,647 integers.
                // At 4 bytes per integer, this demands ~8.5 Gigabytes of perfectly 
                // contiguous, unbroken memory space all at once.   
            try
            {
                // Fixed: Avoid allocating an unreasonably large array that guarantees OutOfMemoryException.
                // Use a safe, bounded size instead of int.MaxValue.
                const int SafeArraySize = 1_000_000; // ~4MB, safe and reasonable for demonstration purposes
                int[] safeArray = new int[SafeArraySize];

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