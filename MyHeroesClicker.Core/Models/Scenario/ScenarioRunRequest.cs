namespace MyHeroesClicker.Core.Models.Scenario;

public sealed class ScenarioRunRequest
{
  public ScenarioRunRequest()
  {
  }

  public ScenarioRunRequest(int iterations)
  {
    Iterations = iterations;
  }

  public int Iterations { get; init; }
}
