using MyHeroesClicker.Core.Models.Scenario;

namespace MyHeroesClicker.Core.Interfaces.Scenarios;

public interface IScenarioCoordinator
{
  Task StopGroupAsync(ScenarioConcurrencyGroup group, CancellationToken cancellationToken);
}
