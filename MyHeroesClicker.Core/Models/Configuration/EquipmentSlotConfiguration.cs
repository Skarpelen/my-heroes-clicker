namespace MyHeroesClicker.Core.Models.Configuration;

/// <summary>
/// Конфигурация слота экипировки для режима сценария.
/// </summary>
public sealed class EquipmentSlotConfiguration
{
  /// <summary>
  /// Номер слота экипировки.
  /// </summary>
  public int SlotNumber { get; init; }

  /// <summary>
  /// Идентификатор предмета, ожидаемого в слоте.
  /// </summary>
  public int? ItemId { get; init; }

  /// <summary>
  /// Должен ли слот быть пустым.
  /// </summary>
  public bool ShouldBeEmpty { get; init; }
}