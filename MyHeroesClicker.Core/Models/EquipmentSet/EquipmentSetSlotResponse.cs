namespace MyHeroesClicker.Core.Models.EquipmentSet;

/// <summary>
/// Данные слота сета экипировки, возвращаемые клиентам API.
/// </summary>
public sealed class EquipmentSetSlotResponse : EquipmentSlotModel
{
  /// <summary>
  /// Идентификатор сета экипировки.
  /// </summary>
  public long EquipmentSetId { get; init; }
}