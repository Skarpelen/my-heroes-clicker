using Microsoft.AspNetCore.Mvc;

namespace MyHeroesClicker.API.Modules.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
  [HttpGet]
  public IActionResult Get()
  {
    return Ok(new
    {
      status = "ready",
      application = "MyHeroesClicker.API"
    });
  }
}
