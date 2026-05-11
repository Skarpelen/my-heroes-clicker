using Microsoft.AspNetCore.Mvc;
using MyHeroesClicker.API.Modules.Application;
using MyHeroesClicker.API.Modules.Services;
using MyHeroesClicker.Core.Models.Scenario;
using MyHeroesClicker.API.Modules.Runtime;
using NLog;

namespace MyHeroesClicker.API.Modules.Controllers;

[ApiController]
[Route("api/scenarios")]
public sealed class ScenariosController : ControllerBase
{
  private readonly Logger _log = LogManager.GetCurrentClassLogger();
  private static readonly string[] ScenarioNames =
  [
    "authentication",
    "farmPreparation",
    "combatPreparation",
    "farmBattle",
    "farmCycle",
    "adventureFarmCycle",
    "warRegistration"
  ];

  private readonly ClickerApiService _clicker;

  public ScenariosController(ClickerApiService clicker)
  {
    _clicker = clicker;
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
      application => application.Scenarios.FarmCycle,
      "Запуск фарма в драке",
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
      application => application.Scenarios.AdventureFarmCycle,
      "Запуск фарма в приключениях",
      "Adventure farm scenario start rejected.",
      cancellationToken);
  }

  [HttpPost("war/start")]
  public async Task<IActionResult> StartWarRegistrationAsync(
    CancellationToken cancellationToken)
  {
    try
    {
      var application = await _clicker.GetApplicationAsync(ScenarioRunOptions.Empty, cancellationToken);

      if (!application.IsRunning)
      {
        _clicker.ResetPause();
      }

      await application.StartScenarioAsync(
        application.Scenarios.WarRegistration,
        ScenarioRunOptions.Empty,
        cancellationToken);
      _clicker.SetLastUserEvent("Запуск авто войны");
      _clicker.ClearLastError();

      return AcceptedAtAction(nameof(GetStatus));
    }
    catch (InvalidOperationException exception)
    {
      _clicker.SetLastError(exception.Message);
      _log.Warn(exception, "War registration scenario start rejected.");

      return Conflict(new { error = exception.Message });
    }
    catch (Exception exception)
    {
      _clicker.SetLastError(exception.Message);
      throw;
    }
  }

  private async Task<IActionResult> StartRunAsync(
    ScenarioRunRequest request,
    Func<ClickerApplication, ScenarioCatalogEntry> selectScenario,
    string userEvent,
    string rejectionLogMessage,
    CancellationToken cancellationToken)
  {
    if (request.Iterations <= 0)
    {
      return BadRequest(new { error = "Iterations must be positive." });
    }

    try
    {
      var runOptions = new ScenarioRunOptions
      {
        IterationLimit = request.Iterations
      };
      var application = await _clicker.GetApplicationAsync(runOptions, cancellationToken);

      if (!application.IsRunning)
      {
        _clicker.ResetPause();
      }

      await application.StartScenarioAsync(selectScenario(application), runOptions, cancellationToken);
      _clicker.SetLastUserEvent(userEvent);
      _clicker.ClearLastError();

      return AcceptedAtAction(nameof(GetStatus));
    }
    catch (InvalidOperationException exception)
    {
      _clicker.SetLastError(exception.Message);
      _log.Warn(exception, rejectionLogMessage);

      return Conflict(new { error = exception.Message });
    }
    catch (Exception exception)
    {
      _clicker.SetLastError(exception.Message);
      throw;
    }
  }

  [HttpPost("farm/prepare")]
  public async Task<IActionResult> StartFarmPreparationAsync(CancellationToken cancellationToken)
  {
    return await StartScenarioAsync(
      application => application.Scenarios.FarmPreparation,
      "Подготовка фарм-сета",
      "Farm preparation scenario start rejected.",
      cancellationToken);
  }

  [HttpPost("combat/prepare")]
  public async Task<IActionResult> StartCombatPreparationAsync(CancellationToken cancellationToken)
  {
    return await StartScenarioAsync(
      application => application.Scenarios.CombatPreparation,
      "Подготовка боевого сета",
      "Combat preparation scenario start rejected.",
      cancellationToken);
  }

  [HttpPost("stop")]
  public IActionResult Stop()
  {
    _clicker.Stop();

    return AcceptedAtAction(nameof(GetStatus));
  }

  [HttpPost("{scenarioKey}/stop")]
  public IActionResult StopScenario(string scenarioKey)
  {
    var stopped = _clicker.StopScenario(scenarioKey);

    return stopped ? AcceptedAtAction(nameof(GetStatus)) : NotFound(new { error = "Сценарий не запущен." });
  }

  [HttpPost("resume")]
  public IActionResult Resume()
  {
    _clicker.Resume();

    return AcceptedAtAction(nameof(GetStatus));
  }

  private async Task<IActionResult> StartScenarioAsync(
    Func<ClickerApplication, ScenarioCatalogEntry> selectScenario,
    string userEvent,
    string rejectionLogMessage,
    CancellationToken cancellationToken)
  {
    try
    {
      var application = await _clicker.GetApplicationAsync(ScenarioRunOptions.Empty, cancellationToken);

      if (!application.IsRunning)
      {
        _clicker.ResetPause();
      }

      await application.StartScenarioAsync(
        selectScenario(application),
        ScenarioRunOptions.Empty,
        cancellationToken);
      _clicker.SetLastUserEvent(userEvent);
      _clicker.ClearLastError();

      return AcceptedAtAction(nameof(GetStatus));
    }
    catch (InvalidOperationException exception)
    {
      _clicker.SetLastError(exception.Message);
      _log.Warn(exception, rejectionLogMessage);

      return Conflict(new { error = exception.Message });
    }
    catch (Exception exception)
    {
      _clicker.SetLastError(exception.Message);
      throw;
    }
  }
}
