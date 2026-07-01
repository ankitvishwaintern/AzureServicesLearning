using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Controllers
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
            int denominator = 0; // This was causing the divide by zero exception

            // Fix: Check for zero before dividing
            if (denominator == 0)
            {
                return BadRequest("Denominator cannot be zero.");
            }

            int result = numerator / denominator;

            var data = new List<object>
            {
                new { Id = 1, Value = result }
            };

            return Ok(data);
        }
    }
}
