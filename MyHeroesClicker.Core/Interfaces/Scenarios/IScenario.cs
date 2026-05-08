using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.Core.Interfaces.Scenarios;

public interface IScenario
{
  string Name { get; }

  Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken);
}
