namespace MyHeroesClicker.Core.Models.Configuration;

/// <summary>
/// Конфигурация экипировки для режимов фарма и боя.
/// </summary>
public sealed class EquipmentModeConfiguration
{
  /// <summary>
  /// Слоты экипировки для фарма.
  /// </summary>
  public IReadOnlyCollection<EquipmentSlotConfiguration> FarmSlots { get; init; } = [];

  /// <summary>
  /// Слоты экипировки для боя.
  /// </summary>
  public IReadOnlyCollection<EquipmentSlotConfiguration> CombatSlots { get; init; } = [];
}