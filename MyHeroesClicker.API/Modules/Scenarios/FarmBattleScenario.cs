using MyHeroesClicker.Browser;
using MyHeroesClicker.Core;
using MyHeroesClicker.Steps;

namespace MyHeroesClicker.Scenarios;

public sealed class FarmBattleScenario : IScenario
{
  private readonly ScenarioRunner _runner;

  public FarmBattleScenario(BattleResourcesReader resourcesReader, IScenario authenticationScenario)
  {
    var blockerHandler = new BattleBlockerHandler(resourcesReader);

    _runner = new ScenarioRunner([
      new EnterBattleStep(),
      new AttackStep(blockerHandler),
      new BattleLogStep()
    ], authenticationScenario);
  }

  public string Name => "Фарм боев";

  public Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    return _runner.RunAsync(context, cancellationToken, ScenarioStepType.EnterBattle);
  }
}
