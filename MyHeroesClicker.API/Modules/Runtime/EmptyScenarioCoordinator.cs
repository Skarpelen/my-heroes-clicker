using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Models.Scenario;

namespace MyHeroesClicker.API.Modules.Runtime;

public sealed class EmptyScenarioCoordinator : IScenarioCoordinator
{
  public static EmptyScenarioCoordinator Instance { get; } = new();

  private EmptyScenarioCoordinator()
  {
  }

  public Task StopGroupAsync(ScenarioConcurrencyGroup group, CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}
