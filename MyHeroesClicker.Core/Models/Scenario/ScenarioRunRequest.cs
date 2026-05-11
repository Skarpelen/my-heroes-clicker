namespace MyHeroesClicker.Core.Models.Scenario;

/// <summary>
/// Запрос на запуск сценария.
/// </summary>
public sealed class ScenarioRunRequest
{
  /// <summary>
  /// Запрошенное количество итераций.
  /// </summary>
  public int Iterations { get; init; }
}