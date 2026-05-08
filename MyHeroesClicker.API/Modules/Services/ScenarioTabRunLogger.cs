using MyHeroesClicker.Core.Interfaces.Services;

namespace MyHeroesClicker.API.Modules.Services;

public sealed class ScenarioTabRunLogger : IRunLogger
{
  private readonly IRunLogger _innerLogger;
  private readonly string _tabName;

  public ScenarioTabRunLogger(IRunLogger innerLogger, string tabName)
  {
    _innerLogger = innerLogger;
    _tabName = tabName;
  }

  public void Log(string message)
  {
    _innerLogger.Log(Format(message));
  }

  public void Warn(string message)
  {
    _innerLogger.Warn(Format(message));
  }

  public void Error(string message)
  {
    _innerLogger.Error(Format(message));
  }

  private string Format(string message)
  {
    return $"[{_tabName}] {message}";
  }
}
