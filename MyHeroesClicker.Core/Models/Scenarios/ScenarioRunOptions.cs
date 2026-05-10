namespace MyHeroesClicker.Core.Models.Scenarios;

public sealed record ScenarioRunOptions(int? IterationLimit = null)
{
  public static ScenarioRunOptions Empty { get; } = new();
}
