namespace MyHeroesClicker.Core.Models.Scenario;

/// <summary>
/// Статус отдельного запуска сценария кликера.
/// </summary>
public sealed class ScenarioRunStatusResponse
{
  /// <summary>
  /// Уникальный идентификатор запуска.
  /// </summary>
  public string RunId { get; init; } = string.Empty;

  /// <summary>
  /// Ключ сценария в каталоге сценариев.
  /// </summary>
  public string ScenarioKey { get; init; } = string.Empty;

  /// <summary>
  /// Отображаемое имя сценария.
  /// </summary>
  public string ScenarioName { get; init; } = string.Empty;

  /// <summary>
  /// Тип вкладки браузера, занятой запуском.
  /// </summary>
  public string BrowserTabKind { get; init; } = string.Empty;

  /// <summary>
  /// Отображаемое имя вкладки браузера.
  /// </summary>
  public string BrowserTabName { get; init; } = string.Empty;

  /// <summary>
  /// Текущее состояние запуска: running, paused, stopping, stopped или failed.
  /// </summary>
  public string State { get; init; } = string.Empty;

  /// <summary>
  /// Время старта запуска.
  /// </summary>
  public DateTimeOffset StartedAt { get; init; }

  /// <summary>
  /// Время запроса остановки запуска.
  /// </summary>
  public DateTimeOffset? StopRequestedAt { get; init; }

  /// <summary>
  /// Ограничение количества итераций.
  /// </summary>
  public int? IterationLimit { get; init; }

  /// <summary>
  /// Количество завершенных итераций.
  /// </summary>
  public int CompletedIterations { get; init; }

  /// <summary>
  /// Прогресс выполнения в процентах.
  /// </summary>
  public int? ProgressPercent { get; init; }

  /// <summary>
  /// Текстовое состояние запуска.
  /// </summary>
  public string? StatusMessage { get; init; }

  /// <summary>
  /// Время следующей запланированной проверки, если сценарий работает по расписанию.
  /// </summary>
  public DateTimeOffset? NextCheckAt { get; init; }

  /// <summary>
  /// Последняя ошибка запуска.
  /// </summary>
  public string? LastError { get; init; }
}
