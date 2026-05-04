using MyHeroesClicker.Core;

namespace MyHeroesClicker.Scenarios;

public sealed class FarmPreparationScenario : IScenario
{
  public string Name => "Подготовка к фарму";

  public Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    // Future flow: authorize, read character info, equip farm items, and disable extra techniques.
    return Task.CompletedTask;
  }
}
