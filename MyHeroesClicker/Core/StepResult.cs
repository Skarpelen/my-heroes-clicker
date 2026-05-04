namespace MyHeroesClicker.Core;

public sealed class StepResult
{
  public StepResult(ScenarioStepType nextStep)
  {
    NextStep = nextStep;
  }

  public ScenarioStepType NextStep { get; }
}