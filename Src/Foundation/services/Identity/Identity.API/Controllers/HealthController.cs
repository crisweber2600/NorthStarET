using Microsoft.AspNetCore.Mvc;

namespace NorthStarET.Foundation.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "Identity Service",
            Timestamp = DateTime.UtcNow
        });
    }
}
