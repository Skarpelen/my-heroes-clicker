using MyHeroesClicker.Core.Interfaces.Services;

namespace MyHeroesClicker.API.Modules.Services;

public sealed class ScenarioRunLogger : IRunLogger
{
  private readonly ILogger<ScenarioRunLogger> _logger;

  public ScenarioRunLogger(ILogger<ScenarioRunLogger> logger)
  {
    _logger = logger;
  }

  public void Log(string message)
  {
    _logger.LogInformation("{Message}", message);
  }

  public void Warn(string message)
  {
    _logger.LogWarning("{Message}", message);
  }

  public void Error(string message)
  {
    _logger.LogError("{Message}", message);
  }
}
