using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.API.Modules.Core;

public sealed class CompositeScenario : IScenario
{
  private readonly IReadOnlyList<IScenario> _scenarios;

  public CompositeScenario(string name, IReadOnlyList<IScenario> scenarios)
  {
    Name = name;
    _scenarios = scenarios;
  }

  public string Name { get; }

  public async Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    foreach (var scenario in _scenarios)
    {
      cancellationToken.ThrowIfCancellationRequested();

      context.Logger.Log($"Выполняется сценарий: {scenario.Name}");
      await scenario.ExecuteAsync(context, cancellationToken);
    }
  }
}
