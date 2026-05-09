using MyHeroesClicker.Core.Interfaces.Services;
using NLog;

namespace MyHeroesClicker.API.Modules.Services;

public sealed class ScenarioRunLogger : IRunLogger
{
  private readonly Logger _log = LogManager.GetCurrentClassLogger();

  public void Log(string message)
  {
    _log.Info(message);
  }

  public void Warn(string message)
  {
    _log.Warn(message);
  }

  public void Error(string message)
  {
    _log.Error(message);
  }
}
