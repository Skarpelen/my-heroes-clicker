namespace MyHeroesClicker.Core.Models.Scenario;

public sealed class ScenarioRunOptions
{
  public ScenarioRunOptions(int? iterationLimit = null)
  {
    IterationLimit = iterationLimit;
  }

  public static ScenarioRunOptions Empty { get; } = new();

  public int? IterationLimit { get; }
}
