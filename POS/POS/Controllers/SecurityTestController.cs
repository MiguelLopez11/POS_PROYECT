using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/[controller]")]
public class RateLimitTestController : ControllerBase
{
    private static int _requestCount = 0;

    [HttpGet("fixed-window")]
    [EnableRateLimiting("StrictPolicy")]
    public IActionResult FixedWindowTest()
    {
        _requestCount++;
        return Ok(new
        {
            message = $"Request #{_requestCount} - Fixed Window",
            timestamp = DateTime.UtcNow,
            policy = "StrictPolicy (3 requests/30 seconds)"
        });
    }

    [HttpGet("sliding-window")]
    [EnableRateLimiting("SlidingPolicy")]
    public IActionResult SlidingWindowTest()
    {
        return Ok(new
        {
            message = "Sliding Window Test",
            timestamp = DateTime.UtcNow,
            policy = "SlidingPolicy (5 requests/30 seconds)"
        });
    }

    [HttpGet("no-limit")]
    public IActionResult NoLimitTest()
    {
        return Ok(new
        {
            message = "No rate limit applied",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("reset-counter")]
    public IActionResult ResetCounter()
    {
        _requestCount = 0;
        return Ok(new { message = "Counter reset to 0" });
    }
}