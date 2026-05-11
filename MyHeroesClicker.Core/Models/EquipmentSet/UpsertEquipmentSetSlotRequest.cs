namespace MyHeroesClicker.Core.Models.EquipmentSet;

public sealed class UpsertEquipmentSetSlotRequest
{
  public UpsertEquipmentSetSlotRequest()
  {
  }

  public UpsertEquipmentSetSlotRequest(int? itemId, string expectedImageSrc, bool shouldBeEmpty)
  {
    ItemId = itemId;
    ExpectedImageSrc = expectedImageSrc;
    ShouldBeEmpty = shouldBeEmpty;
  }

  public int? ItemId { get; init; }

  public string ExpectedImageSrc { get; init; } = string.Empty;

  public bool ShouldBeEmpty { get; init; }
}
