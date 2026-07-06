using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SampleDataController : ControllerBase
    {
        private readonly ILogger<SampleDataController> _logger;
        private readonly TelemetryClient _telemetryClient;
        private static readonly ActivitySource ActivitySource = new("SampleDataController");

        public SampleDataController(ILogger<SampleDataController> logger, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Returns a small set of sample data.
        /// GET /api/sampledata
        /// </summary>
        [HttpGet("SampleData")]
        public IEnumerable<DataItem> Get()
                    _logger.LogInformation("SampleData endpoint called");
                {
                    int a = 9;
                    int b = 0;
                    int c = a / b;
                    _logger.LogInformation("SampleData endpoint called");

                    activity?.SetTag("data.count", 3);
                    activity?.SetTag("data.source", "in-memory");

                    // Track custom event in Azure Application Insights
                    var properties = new Dictionary<string, string>
                    {
                        { "endpoint", "GetSampleData" },
                        { "data.source", "in-memory" },
                        { "data.count", "3" }
                    };
                    _telemetryClient.TrackEvent("GetSampleData_Called", properties);

                    var data = new[]
                    {
                        new DataItem(1, "Alpha", DateTime.UtcNow),
                        new DataItem(2, "Beta", DateTime.UtcNow.AddMinutes(-5)),
                        new DataItem(3, "Gamma", DateTime.UtcNow.AddHours(-1))
                    };

                    _logger.LogInformation("Successfully returned {count} data items", data.Length);
                    
                    return data;
                }
            }
            catch (Exception ex)
            {
                using (var activity = ActivitySource.StartActivity("GetSampleData-Error"))
                {
                    activity?.SetTag("exception.type", ex.GetType().Name);
                    activity?.SetTag("exception.message", ex.Message);
                    activity?.SetTag("exception.stacktrace", ex.StackTrace);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                    _logger.LogError(ex, "An error occurred while retrieving sample data. Stack trace: {stackTrace}", ex.StackTrace);

                    // Track exception in Azure Application Insights
                    var exceptionTelemetry = new ExceptionTelemetry(ex)
                    {
                        SeverityLevel = SeverityLevel.Error
                    };
                    exceptionTelemetry.Properties.Add("endpoint", "GetSampleData");
                    exceptionTelemetry.Properties.Add("exception.stacktrace", ex.StackTrace ?? "No stack trace");
                    _telemetryClient.TrackException(exceptionTelemetry);
                }
                return Array.Empty<DataItem>();
            }
        }

        /// <summary>
        /// Test endpoint for verifying global exception handler behavior.
        /// GET /api/sampledata/test-exception
        /// </summary>
        [HttpGet("test-exception")]
        public IActionResult TestException()
        {
            _logger.LogInformation("Test exception endpoint called");
            
            using (var activity = ActivitySource.StartActivity("TestException"))
            {
                activity?.SetTag("test.purpose", "global-exception-handler");
                activity?.SetTag("endpoint", "TestException");

                var properties = new Dictionary<string, string>
                {
                    { "endpoint", "TestException" },
                    { "purpose", "global-exception-handler-test" }
                };
                _telemetryClient.TrackEvent("TestException_Called", properties);

                // Intentionally throw an exception to test the global exception handler
                throw new InvalidOperationException("This is a test exception to verify global exception handler is working correctly.");
            }
        }

        /// <summary>
        /// Names
        [HttpGet("names")]
        public IEnumerable<string> GetNames()
        {
            _logger.LogInformation("GetNames endpoint called");
            using (var activity = ActivitySource.StartActivity("GetNames"))
            {
                string text = "Hello";
                char c = text[10];
                activity?.SetTag("data.count", 3);
                activity?.SetTag("data.source", "in-memory");
                var properties = new Dictionary<string, string>
                {
                    { "endpoint", "GetNames" },
                    { "data.source", "in-memory" },
                    { "data.count", "3" }
                };
                _telemetryClient.TrackEvent("GetNames_Called", properties);
                return new[] { "Alpha", "Beta", "Gamma" };
            }
        }
    }



    public sealed record DataItem(int Id, string Name, DateTime CreatedAt);
}