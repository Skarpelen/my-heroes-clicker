namespace MyHeroesClicker.Core.Models.Scenario;

public sealed class WarScenarioState
{
  public WarScenarioState(
    WarScenarioStateKind kind,
    DateTimeOffset? warEndsAt = null,
    DateTimeOffset? nextBattleAt = null,
    TimeSpan? battleStartsIn = null)
  {
    Kind = kind;
    WarEndsAt = warEndsAt;
    NextBattleAt = nextBattleAt;
    BattleStartsIn = battleStartsIn;
  }

  public WarScenarioStateKind Kind { get; }

  public DateTimeOffset? WarEndsAt { get; }

  public DateTimeOffset? NextBattleAt { get; }

  public TimeSpan? BattleStartsIn { get; }
}
