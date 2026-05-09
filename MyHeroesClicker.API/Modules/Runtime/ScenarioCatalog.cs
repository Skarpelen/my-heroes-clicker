using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Models.Scenarios;

namespace MyHeroesClicker.API.Modules.Runtime;

public sealed class ScenarioCatalog
{
  public ScenarioCatalog(
    IScenario authentication,
    IScenario farmPreparation,
    IScenario combatPreparation,
    IScenario farmBattle,
    IScenario farmCycle,
    IScenario adventureFarmCycle)
  {
    Authentication = new ScenarioCatalogEntry(
      "authentication",
      authentication,
      ScenarioExecutionMode.Http,
      ScenarioBrowserTabKind.Main,
      "Основная вкладка");
    FarmPreparation = new ScenarioCatalogEntry(
      "farmPreparation",
      farmPreparation,
      ScenarioExecutionMode.Http,
      ScenarioBrowserTabKind.Main,
      "Основная вкладка");
    CombatPreparation = new ScenarioCatalogEntry(
      "combatPreparation",
      combatPreparation,
      ScenarioExecutionMode.Http,
      ScenarioBrowserTabKind.Main,
      "Основная вкладка");
    FarmBattle = new ScenarioCatalogEntry(
      "farmBattle",
      farmBattle,
      ScenarioExecutionMode.Playwright,
      ScenarioBrowserTabKind.BattleFarm,
      "Фарм боев",
      ScenarioConcurrencyGroup.Farm);
    FarmCycle = new ScenarioCatalogEntry(
      "farmCycle",
      farmCycle,
      ScenarioExecutionMode.Mixed,
      ScenarioBrowserTabKind.BattleFarm,
      "Фарм боев",
      ScenarioConcurrencyGroup.Farm);
    AdventureFarmCycle = new ScenarioCatalogEntry(
      "adventureFarmCycle",
      adventureFarmCycle,
      ScenarioExecutionMode.Mixed,
      ScenarioBrowserTabKind.AdventureFarm,
      "Фарм приключений",
      ScenarioConcurrencyGroup.Farm);
  }

  public ScenarioCatalogEntry Authentication { get; }

  public ScenarioCatalogEntry FarmPreparation { get; }

  public ScenarioCatalogEntry CombatPreparation { get; }

  public ScenarioCatalogEntry FarmBattle { get; }

  public ScenarioCatalogEntry FarmCycle { get; }

  public ScenarioCatalogEntry AdventureFarmCycle { get; }
}
