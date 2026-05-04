namespace MyHeroesClicker.Core;

public interface IScenarioStep
{
  ScenarioStepKind Kind { get; }

  Task<StepResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken);
}