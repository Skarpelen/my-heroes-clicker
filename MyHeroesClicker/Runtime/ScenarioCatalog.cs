using MyHeroesClicker.Core;

namespace MyHeroesClicker.Runtime;

public sealed class ScenarioCatalog
{
  public ScenarioCatalog(
    IScenario authentication,
    IScenario farmPreparation,
    IScenario combatPreparation,
    IScenario farmBattle,
    IScenario farmCycle)
  {
    Authentication = authentication;
    FarmPreparation = farmPreparation;
    CombatPreparation = combatPreparation;
    FarmBattle = farmBattle;
    FarmCycle = farmCycle;
  }

  public IScenario Authentication { get; }

  public IScenario FarmPreparation { get; }

  public IScenario CombatPreparation { get; }

  public IScenario FarmBattle { get; }

  public IScenario FarmCycle { get; }
}
