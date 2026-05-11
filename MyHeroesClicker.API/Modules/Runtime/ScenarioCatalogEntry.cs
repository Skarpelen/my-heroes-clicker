using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Models.Scenario;

namespace MyHeroesClicker.API.Modules.Runtime;

public sealed class ScenarioCatalogEntry
{
  public ScenarioCatalogEntry(
    string key,
    IScenario scenario,
    ScenarioExecutionMode executionMode,
    ScenarioBrowserTabKind browserTabKind,
    string browserTabName,
    ScenarioConcurrencyGroup? concurrencyGroup = null)
  {
    Key = key;
    Scenario = scenario;
    ExecutionMode = executionMode;
    BrowserTabKind = browserTabKind;
    BrowserTabName = browserTabName;
    ConcurrencyGroup = concurrencyGroup;
  }

  public string Key { get; }

  public IScenario Scenario { get; }

  public ScenarioExecutionMode ExecutionMode { get; }

  public ScenarioBrowserTabKind BrowserTabKind { get; }

  public string BrowserTabName { get; }

  public ScenarioConcurrencyGroup? ConcurrencyGroup { get; }
}
