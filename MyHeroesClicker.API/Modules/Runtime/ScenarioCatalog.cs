using MyHeroesClicker.Core;

namespace MyHeroesClicker.Runtime;

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
    Authentication = authentication;
    FarmPreparation = farmPreparation;
    CombatPreparation = combatPreparation;
    FarmBattle = farmBattle;
    FarmCycle = farmCycle;
    AdventureFarmCycle = adventureFarmCycle;
  }

  public IScenario Authentication { get; }

  public IScenario FarmPreparation { get; }

  public IScenario CombatPreparation { get; }

  public IScenario FarmBattle { get; }

  public IScenario FarmCycle { get; }

  public IScenario AdventureFarmCycle { get; }
}
