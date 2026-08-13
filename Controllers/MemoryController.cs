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
            try
            {
                // Generate a reasonably sized report instead of an impossible allocation.
                const int reportSizeBytes = 1024 * 1024; // 1 MB
                byte[] report = new byte[reportSizeBytes];

                // Prevent unbounded growth of the static cache (Issue 1 mitigation):
                // cap the cache size so it doesn't grow indefinitely and leak memory.
                const int maxCachedReports = 10;
                if (_globalReportCache.Count >= maxCachedReports)
                {
                    _globalReportCache.RemoveAt(0);
                }
                _globalReportCache.Add(report);

                return Ok(new { Message = "Report generated successfully.", Length = report.Length });
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                throw;
            }
        }
    }
        
}