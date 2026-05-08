using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Models.Scenarios;

namespace MyHeroesClicker.API.Modules.Runtime;

public sealed record ScenarioCatalogEntry(
  string Key,
  IScenario Scenario,
  ScenarioExecutionMode ExecutionMode,
  ScenarioBrowserTabKind BrowserTabKind,
  string BrowserTabName);
