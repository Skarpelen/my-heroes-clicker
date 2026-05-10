using MyHeroesClicker.Core.Models.Scenarios;

namespace MyHeroesClicker.Core.Interfaces.Scenarios;

public interface IScenarioCoordinator
{
  Task StopGroupAsync(ScenarioConcurrencyGroup group, CancellationToken cancellationToken);
}
