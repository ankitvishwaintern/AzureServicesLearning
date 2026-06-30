       public IActionResult Get()
       {
           int a = ...; // some value
           int b = ...; // some value, could be zero
           if (b == 0)
           {
               // Log error, return HTTP 400 Bad Request with meaningful message
               _logger.LogError("Attempted to divide by zero in Get(). a={A}", a);
               return BadRequest("Invalid input: divisor cannot be zero.");
           }
           int c = a / b; // Safe division
           // other logic
           return Ok(c);
       }