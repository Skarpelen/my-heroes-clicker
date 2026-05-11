namespace MyHeroesClicker.Core.Models.Configuration;

public sealed class EquipmentModeConfiguration
{
  public EquipmentModeConfiguration(
    IReadOnlyCollection<EquipmentSlotConfiguration> farmSlots,
    IReadOnlyCollection<EquipmentSlotConfiguration> combatSlots)
  {
    FarmSlots = farmSlots;
    CombatSlots = combatSlots;
  }

  public IReadOnlyCollection<EquipmentSlotConfiguration> FarmSlots { get; }

  public IReadOnlyCollection<EquipmentSlotConfiguration> CombatSlots { get; }
}
