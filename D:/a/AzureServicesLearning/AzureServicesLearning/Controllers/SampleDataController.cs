using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace AzureServicesLearning.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleDataController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                int numerator = 100;
                int denominator = GetDenominator(); // Example method to get denominator

                if (denominator == 0)
                {
                    return BadRequest("Denominator cannot be zero.");
                }

                int result = numerator / denominator;
                return Ok(new { Result = result });
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return StatusCode(500, $"An error occurred while retrieving sample data. {ex.Message}");
            }
        }

        private int GetDenominator()
        {
            // Replace this with actual logic to get the denominator
            // For demonstration, returning a non-zero value
            return 1;
        }
    }
}
