namespace MyHeroesClicker.Core.Models.Scenario;

/// <summary>
/// Режим выполнения сценария.
/// </summary>
public enum ScenarioExecutionMode
{
  /// <summary>
  /// Выполнение только через HTTP.
  /// </summary>
  Http,

  /// <summary>
  /// Выполнение только через Playwright.
  /// </summary>
  Playwright,

  /// <summary>
  /// Смешанное выполнение через HTTP и Playwright.
  /// </summary>
  Mixed
}