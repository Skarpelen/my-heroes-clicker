namespace MyHeroesClicker.Core.Models.Scenario;

/// <summary>
/// Текущий статус сценариев кликера, возвращаемый клиентам API.
/// </summary>
public sealed class ScenarioStatusResponse
{
  /// <summary>
  /// Инициализирована ли среда выполнения.
  /// </summary>
  public bool IsInitialized { get; init; }

  /// <summary>
  /// Выполняется ли какой-либо сценарий.
  /// </summary>
  public bool IsRunning { get; init; }

  /// <summary>
  /// Приостановлено ли выполнение.
  /// </summary>
  public bool IsPaused { get; init; }

  /// <summary>
  /// Ключ активного сценария.
  /// </summary>
  public string? ActiveScenarioKey { get; init; }

  /// <summary>
  /// Название активного сценария.
  /// </summary>
  public string? ActiveScenarioName { get; init; }

  /// <summary>
  /// Название активной вкладки браузера.
  /// </summary>
  public string? BrowserTabName { get; init; }

  /// <summary>
  /// Ключи всех выполняющихся сценариев.
  /// </summary>
  public IReadOnlyCollection<string> RunningScenarioKeys { get; init; } = [];

  /// <summary>
  /// Причину паузы.
  /// </summary>
  public string? PauseReason { get; init; }

  /// <summary>
  /// Время запроса паузы.
  /// </summary>
  public DateTimeOffset? PauseRequestedAt { get; init; }

  /// <summary>
  /// Название последнего пользовательского события.
  /// </summary>
  public string? LastUserEvent { get; init; }

  /// <summary>
  /// Время последнего пользовательского события.
  /// </summary>
  public DateTimeOffset? LastUserEventAt { get; init; }

  /// <summary>
  /// Ограничение количества итераций.
  /// </summary>
  public int? IterationLimit { get; init; }

  /// <summary>
  /// Количество завершенных итераций.
  /// </summary>
  public int? CompletedIterations { get; init; }

  /// <summary>
  /// Максимальное известное здоровье персонажа.
  /// </summary>
  public int? MaxHealth { get; init; }

  /// <summary>
  /// Текст состояния сценария войны.
  /// </summary>
  public string? WarState { get; init; }

  /// <summary>
  /// Время следующей запланированной проверки войны.
  /// </summary>
  public DateTimeOffset? WarNextCheckAt { get; init; }

  /// <summary>
  /// Последнюю ошибку времени выполнения.
  /// </summary>
  public string? LastError { get; init; }
}