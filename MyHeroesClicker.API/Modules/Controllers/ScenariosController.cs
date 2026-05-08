using Microsoft.AspNetCore.Mvc;
using MyHeroesClicker.API.Contracts;
using MyHeroesClicker.API.Services;

namespace MyHeroesClicker.API.Modules.Controllers;

[ApiController]
[Route("api/scenarios")]
public sealed class ScenariosController : ControllerBase
{
  private static readonly string[] ScenarioNames =
  [
    "authentication",
    "farmPreparation",
    "combatPreparation",
    "farmBattle",
    "farmCycle",
    "adventureFarmCycle"
  ];

  private readonly ClickerApiService _clicker;
  private readonly ILogger<ScenariosController> _logger;

  public ScenariosController(
    ClickerApiService clicker,
    ILogger<ScenariosController> logger)
  {
    _clicker = clicker;
    _logger = logger;
  }

  [HttpGet]
  public ActionResult<IReadOnlyCollection<string>> GetScenarios()
  {
    return Ok(ScenarioNames);
  }

  [HttpGet("status")]
  public ActionResult<ScenarioStatusResponse> GetStatus()
  {
    return Ok(_clicker.GetStatus());
  }

  [HttpPost("farm/start")]
  public async Task<IActionResult> StartFarmAsync(
    [FromBody] ScenarioRunRequest request,
    CancellationToken cancellationToken)
  {
    return await StartRunAsync(
      request,
      clicker => clicker.StartFarmAsync(request, cancellationToken),
      "Farm scenario start rejected.",
      cancellationToken);
  }

  [HttpPost("adventure/start")]
  public async Task<IActionResult> StartAdventureFarmAsync(
    [FromBody] ScenarioRunRequest request,
    CancellationToken cancellationToken)
  {
    return await StartRunAsync(
      request,
      clicker => clicker.StartAdventureFarmAsync(request, cancellationToken),
      "Adventure farm scenario start rejected.",
      cancellationToken);
  }

  private async Task<IActionResult> StartRunAsync(
    ScenarioRunRequest request,
    Func<ClickerApiService, Task> startRun,
    string rejectionLogMessage,
    CancellationToken cancellationToken)
  {
    try
    {
      await startRun(_clicker);

      return AcceptedAtAction(nameof(GetStatus));
    }
    catch (ArgumentOutOfRangeException exception)
    {
      _logger.LogWarning(exception, "Invalid farm scenario request.");

      return BadRequest(new { error = exception.Message });
    }
    catch (InvalidOperationException exception)
    {
      _logger.LogWarning(exception, rejectionLogMessage);

      return Conflict(new { error = exception.Message });
    }
  }

  [HttpPost("farm/prepare")]
  public async Task<IActionResult> StartFarmPreparationAsync(CancellationToken cancellationToken)
  {
    return await StartScenarioAsync(
      clicker => clicker.StartFarmPreparationAsync(cancellationToken),
      "Farm preparation scenario start rejected.");
  }

  [HttpPost("combat/prepare")]
  public async Task<IActionResult> StartCombatPreparationAsync(CancellationToken cancellationToken)
  {
    return await StartScenarioAsync(
      clicker => clicker.StartCombatPreparationAsync(cancellationToken),
      "Combat preparation scenario start rejected.");
  }

  [HttpPost("stop")]
  public IActionResult Stop()
  {
    _clicker.Stop();

    return AcceptedAtAction(nameof(GetStatus));
  }

  [HttpPost("resume")]
  public IActionResult Resume()
  {
    _clicker.Resume();

    return AcceptedAtAction(nameof(GetStatus));
  }

  private async Task<IActionResult> StartScenarioAsync(
    Func<ClickerApiService, Task> startScenario,
    string rejectionLogMessage)
  {
    try
    {
      await startScenario(_clicker);

      return AcceptedAtAction(nameof(GetStatus));
    }
    catch (InvalidOperationException exception)
    {
      _logger.LogWarning(exception, rejectionLogMessage);

      return Conflict(new { error = exception.Message });
    }
  }
}
