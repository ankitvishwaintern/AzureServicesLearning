using Microsoft.AspNetCore.Mvc;
using System;

namespace Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleDataController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            // Example variables for demonstration
            int numerator = 10;
            int denominator = 0; // This was causing the divide by zero

            // Fix: Check for zero denominator
            if (denominator == 0)
            {
                return BadRequest("Denominator cannot be zero.");
            }

            int result = numerator / denominator;
            return Ok(new { Result = result });
        }
    }
}