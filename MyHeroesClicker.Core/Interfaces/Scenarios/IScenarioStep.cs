namespace MyHeroesClicker.Core;

public interface IScenarioStep
{
  ScenarioStepType Type { get; }

  Task<bool> CanHandleAsync(ScenarioContext context, CancellationToken cancellationToken);

  Task<StepResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken);
}
