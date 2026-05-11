namespace MyHeroesClicker.Core.Models.Alert;

/// <summary>
/// Событие оповещения, публикуемое внутри приложения.
/// </summary>
public sealed class AlertEvent : AlertEventModel
{
  /// <summary>
  /// Тип оповещения.
  /// </summary>
  public AlertEventKind Kind { get; init; }
}