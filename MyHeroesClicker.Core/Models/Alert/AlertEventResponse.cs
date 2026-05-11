namespace MyHeroesClicker.Core.Models.Alert;

/// <summary>
/// Данные события оповещения, возвращаемые клиентам API.
/// </summary>
public sealed class AlertEventResponse : AlertEventModel
{
  /// <summary>
  /// Тип оповещения для API.
  /// </summary>
  public string Kind { get; init; } = string.Empty;
}