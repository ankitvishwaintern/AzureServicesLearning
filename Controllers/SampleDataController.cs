using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace AzureServicesLearning.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleDataController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            // Example data
            int numerator = 10;
            int denominator = 0; // This was causing the exception

            // Fix: Check denominator before division
            int result;
            if (denominator == 0)
            {
                // Return a BadRequest or handle as appropriate
                return BadRequest("Denominator cannot be zero.");
            }
            else
            {
                result = numerator / denominator;
            }

            var data = new List<object>
            {
                new { Id = 1, Value = "Sample 1" },
                new { Id = 2, Value = "Sample 2" },
                new { Id = 3, Value = "Sample 3" },
                new { Calculation = result }
            };

            return Ok(data);
        }
    }
}
