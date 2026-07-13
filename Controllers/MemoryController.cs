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
            // caused an immediate OutOfMemoryException. Use a safe, bounded size instead.
            const int SafeArraySize = 1000; // Small, bounded allocation for demonstration purposes
            try
            {
                int[] safeArray = new int[SafeArraySize];

                // Defensive: cap the static cache size to prevent unbounded memory growth (Issue 1 fix).
                const int MaxCacheEntries = 50;
                var reportBytes = new byte[SafeArraySize * sizeof(int)];
                Buffer.BlockCopy(safeArray, 0, reportBytes, 0, reportBytes.Length);

                lock (_globalReportCache)
                {
                    if (_globalReportCache.Count >= MaxCacheEntries)
                    {
                        _globalReportCache.RemoveAt(0);
                    }
                    _globalReportCache.Add(reportBytes);
                }

                return Ok(new { Message = "Report generated safely.", Length = safeArray.Length });
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                throw;
            }
        }
    }
        
}