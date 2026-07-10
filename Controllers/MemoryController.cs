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
                // Fix: Avoid allocating an impossibly large contiguous block of memory.
                // Use a reasonably sized buffer instead of int.MaxValue elements.
                const int SafeArraySize = 1024 * 1024; // 1 million ints (~4MB), safe and bounded.
                int[] reportArray = new int[SafeArraySize];

                // Fix: Prevent unbounded growth of the static cache by capping its size
                // and evicting the oldest entries (bounded FIFO cache) instead of growing forever.
                var buffer = new byte[SafeArraySize / 8];
                lock (_globalReportCache)
                {
                    const int MaxCachedItems = 10;
                    if (_globalReportCache.Count >= MaxCachedItems)
                    {
                        _globalReportCache.RemoveAt(0);
                    }
                    _globalReportCache.Add(buffer);
                }

                return Ok(new { Message = "Report generated successfully.", Length = reportArray.Length });
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                throw;
            }
        }
    }
        
}