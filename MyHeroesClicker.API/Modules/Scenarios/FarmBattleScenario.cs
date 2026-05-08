using MyHeroesClicker.API.Modules.Core;
using MyHeroesClicker.Browser.Browser;
using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Models.Scenarios;
using MyHeroesClicker.Core.Modules.Core;
using MyHeroesClicker.Steps;

namespace MyHeroesClicker.API.Modules.Scenarios;

public sealed class FarmBattleScenario : IScenario
{
  private readonly ScenarioRunner _runner;
  private readonly string _name;

  public FarmBattleScenario(BattleResourcesReader resourcesReader, IScenario authenticationScenario)
    : this(resourcesReader, authenticationScenario, FarmLocation.Battle)
  {
  }

  public FarmBattleScenario(
    BattleResourcesReader resourcesReader,
    IScenario authenticationScenario,
    FarmLocation location)
  {
    var blockerHandler = new BattleBlockerHandler(resourcesReader, location);
    _name = location == FarmLocation.Adventure
      ? "Фарм приключений"
      : "Фарм боев";

    _runner = new ScenarioRunner([
      new EnterBattleStep(location),
      new AttackStep(blockerHandler, location),
      new BattleLogStep(location)
    ], authenticationScenario);
  }

  public string Name => _name;

  public Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    return _runner.RunAsync(context, cancellationToken, ScenarioStepType.EnterBattle);
  }
}
