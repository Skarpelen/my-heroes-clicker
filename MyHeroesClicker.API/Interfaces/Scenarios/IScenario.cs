namespace MyHeroesClicker.Core;

public interface IScenario
{
  string Name { get; }

  Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken);
}
