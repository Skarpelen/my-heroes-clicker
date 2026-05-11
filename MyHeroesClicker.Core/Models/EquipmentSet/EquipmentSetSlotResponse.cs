namespace MyHeroesClicker.Core.Models.EquipmentSet;

public sealed class EquipmentSetSlotResponse
{
  public EquipmentSetSlotResponse(
    long equipmentSetId,
    int slotNumber,
    int? itemId,
    string expectedImageSrc,
    bool shouldBeEmpty)
  {
    EquipmentSetId = equipmentSetId;
    SlotNumber = slotNumber;
    ItemId = itemId;
    ExpectedImageSrc = expectedImageSrc;
    ShouldBeEmpty = shouldBeEmpty;
  }

  public long EquipmentSetId { get; }

  public int SlotNumber { get; }

  public int? ItemId { get; }

  public string ExpectedImageSrc { get; }

  public bool ShouldBeEmpty { get; }
}
