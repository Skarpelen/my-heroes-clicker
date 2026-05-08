namespace MyHeroesClicker.Core.Models.Scenarios;

public sealed class StepResult
{
  public StepResult(ScenarioStepType nextStep)
  {
    NextStep = nextStep;
  }

  public ScenarioStepType NextStep { get; }
}
