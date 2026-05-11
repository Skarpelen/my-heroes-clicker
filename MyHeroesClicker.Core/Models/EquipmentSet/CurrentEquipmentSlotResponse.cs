namespace MyHeroesClicker.Core.Models.EquipmentSet;

public sealed class CurrentEquipmentSlotResponse
{
  public CurrentEquipmentSlotResponse(
    int slotNumber,
    int? itemId,
    string expectedImageSrc,
    bool shouldBeEmpty)
  {
    SlotNumber = slotNumber;
    ItemId = itemId;
    ExpectedImageSrc = expectedImageSrc;
    ShouldBeEmpty = shouldBeEmpty;
  }

  public int SlotNumber { get; }

  public int? ItemId { get; }

  public string ExpectedImageSrc { get; }

  public bool ShouldBeEmpty { get; }
}
