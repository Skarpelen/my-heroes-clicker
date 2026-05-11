namespace MyHeroesClicker.Core.Models.Scenario;

/// <summary>
/// Результат выполнения шага сценария.
/// </summary>
public sealed class StepResult
{
  /// <summary>
  /// Следующий шаг для выполнения.
  /// </summary>
  public ScenarioStepType NextStep { get; init; }
}