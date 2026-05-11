namespace MyHeroesClicker.Core.Models.Scenario;

/// <summary>
/// Распознанное состояние страницы сценария войны.
/// </summary>
public sealed class WarScenarioState
{
  /// <summary>
  /// Тип состояния.
  /// </summary>
  public WarScenarioStateKind Kind { get; init; }

  /// <summary>
  /// Время окончания текущей войны.
  /// </summary>
  public DateTimeOffset? WarEndsAt { get; init; }

  /// <summary>
  /// Время доступности следующего боя.
  /// </summary>
  public DateTimeOffset? NextBattleAt { get; init; }

  /// <summary>
  /// Оставшееся время до начала боя.
  /// </summary>
  public TimeSpan? BattleStartsIn { get; init; }
}