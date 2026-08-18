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
                // Use a bounded, reasonable allocation size instead of int.MaxValue
                // to avoid guaranteed OutOfMemoryException while still simulating a large report.
                const int MaxSafeArraySize = 10_000_000; // ~40MB, safe for typical server memory
                int[] massiveArray = new int[MaxSafeArraySize];

                // Avoid unbounded growth of the static cache: cap its size and evict oldest entries.
                const int MaxCachedReports = 10;
                if (_globalReportCache.Count >= MaxCachedReports)
                {
                    _globalReportCache.RemoveAt(0);
                }
                _globalReportCache.Add(new byte[1024]); // small placeholder instead of large buffers

                return Ok(new { Message = "Report generated successfully.", Length = massiveArray.Length });
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                throw;
            }
        }
    }
        
}