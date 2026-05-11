namespace MyHeroesClicker.Core.Models.Scenario;

/// <summary>
/// Тип шага боевого сценария.
/// </summary>
public enum ScenarioStepType
{
  /// <summary>
  /// Шаг входа в бой.
  /// </summary>
  EnterBattle,

  /// <summary>
  /// Шаг атаки.
  /// </summary>
  Attack,

  /// <summary>
  /// Шаг лога боя.
  /// </summary>
  BattleLog,

  /// <summary>
  /// Шаг остановки выполнения.
  /// </summary>
  Stop
}