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
                // Force an instant crash on the very first hit.
                // int.MaxValue attempts to create an array with 2,147,483,647 integers.
                // At 4 bytes per integer, this demands ~8.5 Gigabytes of perfectly 
                // contiguous, unbroken memory space all at once.
                int[] massiveArray = new int[int.MaxValue];

                return Ok(new { Message = "This line will never be reached.", Length = massiveArray.Length });
            } 
            catch(Exception ex) 
            {
                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                using (var activity = ActivitySource.StartActivity("GenerateBigReport-Error"))
                {
                    activity?.SetTag("exception.type", ex.GetType().Name);
                    activity?.SetTag("exception.message", ex.Message);
                    activity?.SetTag("exception.stacktrace", ex.StackTrace);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                    _logger.LogError(ex, "An error occurred while retrieving sample data. Stack trace: {stackTrace}", ex.StackTrace);

                    // Track exception in Azure Application Insights
                    
                    exceptionTelemetry.Properties.Add("endpoint", "GenerateBigReport");
                    exceptionTelemetry.Properties.Add("exception.stacktrace", ex.StackTrace ?? "No stack trace");
                    _telemetryClient.TrackException(exceptionTelemetry);
                }
                return Ok(exceptionTelemetry);
            }
        }
    }
        
}
