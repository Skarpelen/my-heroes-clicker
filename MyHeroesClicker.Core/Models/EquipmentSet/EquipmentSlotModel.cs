namespace MyHeroesClicker.Core.Models.EquipmentSet;

/// <summary>
/// Базовая модель слота экипировки.
/// </summary>
public abstract class EquipmentSlotModel
{
  /// <summary>
  /// Номер слота экипировки.
  /// </summary>
  public int SlotNumber { get; init; }

  /// <summary>
  /// Идентификатор предмета в слоте.
  /// </summary>
  public int? ItemId { get; init; }

  /// <summary>
  /// Ожидаемый источник изображения предмета.
  /// </summary>
  public string ExpectedImageSrc { get; init; } = string.Empty;

  /// <summary>
  /// Должен ли слот быть пустым.
  /// </summary>
  public bool ShouldBeEmpty { get; init; }
}