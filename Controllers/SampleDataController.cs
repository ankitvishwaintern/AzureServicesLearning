using Microsoft.AspNetCore.Mvc;
using System;

namespace AzureServicesLearning.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleDataController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(int numerator = 10, int denominator = 1)
        {
            try
            {
                if (denominator == 0)
                {
                    return BadRequest("Denominator cannot be zero.");
                }
                var result = numerator / denominator;
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
