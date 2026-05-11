using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Models.Scenario;

namespace MyHeroesClicker.API.Modules.Runtime;

public sealed class ScenarioCatalog
{
  public ScenarioCatalog(
    IScenario authentication,
    IScenario farmPreparation,
    IScenario combatPreparation,
    IScenario farmBattle,
    IScenario farmCycle,
    IScenario adventureFarmCycle,
    IScenario warRegistration)
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
    WarRegistration = new ScenarioCatalogEntry(
      "warRegistration",
      warRegistration,
      ScenarioExecutionMode.Http,
      ScenarioBrowserTabKind.ClanWar,
      "Войны");
  }

  public ScenarioCatalogEntry Authentication { get; }

  public ScenarioCatalogEntry FarmPreparation { get; }

  public ScenarioCatalogEntry CombatPreparation { get; }

  public ScenarioCatalogEntry FarmBattle { get; }

  public ScenarioCatalogEntry FarmCycle { get; }

  public ScenarioCatalogEntry AdventureFarmCycle { get; }

  public ScenarioCatalogEntry WarRegistration { get; }
}
