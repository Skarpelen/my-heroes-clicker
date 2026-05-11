namespace MyHeroesClicker.Core.Models.Scenario;

/// <summary>
/// Параметры запуска сценария.
/// </summary>
public sealed class ScenarioRunOptions
{
  /// <summary>
  /// Пустые параметры запуска.
  /// </summary>
  public static ScenarioRunOptions Empty { get; } = new ScenarioRunOptions { };

  /// <summary>
  /// Ограничение количества итераций.
  /// </summary>
  public int? IterationLimit { get; init; }
}
