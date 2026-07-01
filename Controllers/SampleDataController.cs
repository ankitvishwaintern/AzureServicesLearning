using Microsoft.AspNetCore.Mvc;
using System;

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
                int numerator = 10;
                int denominator = 0; // Example value, replace with actual logic or input

                if (denominator == 0)
                {
                    return BadRequest("Denominator cannot be zero.");
                }

                int result = numerator / denominator;
                return Ok(new { Result = result });
            }
            catch (Exception ex)
            {
                // Optionally log the exception here
                return StatusCode(500, $"An error occurred while retrieving sample data. {ex.Message}");
            }
        }
    }
}
