namespace MyHeroesClicker.Core.Models.Configuration;

public sealed class EquipmentSlotConfiguration
{
  public EquipmentSlotConfiguration(int slotNumber, int? itemId, bool shouldBeEmpty)
  {
    SlotNumber = slotNumber;
    ItemId = itemId;
    ShouldBeEmpty = shouldBeEmpty;
  }

  public int SlotNumber { get; }

  public int? ItemId { get; }

  public bool ShouldBeEmpty { get; }
}
