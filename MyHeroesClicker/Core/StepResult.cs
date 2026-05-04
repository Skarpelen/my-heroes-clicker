namespace MyHeroesClicker.Core;

public sealed class StepResult
{
  public StepResult(ScenarioStepKind nextStep)
  {
    NextStep = nextStep;
  }

  public ScenarioStepKind NextStep { get; }
}