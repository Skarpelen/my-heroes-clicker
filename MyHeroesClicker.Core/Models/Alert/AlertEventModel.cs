namespace MyHeroesClicker.Core.Models.Alert;

/// <summary>
/// Базовая модель события оповещения.
/// </summary>
public abstract class AlertEventModel
{
  /// <summary>
  /// Идентификатор оповещения.
  /// </summary>
  public long Id { get; init; }

  /// <summary>
  /// Сообщение оповещения.
  /// </summary>
  public string Message { get; init; } = string.Empty;

  /// <summary>
  /// Время создания оповещения.
  /// </summary>
  public DateTimeOffset CreatedAt { get; init; }
}