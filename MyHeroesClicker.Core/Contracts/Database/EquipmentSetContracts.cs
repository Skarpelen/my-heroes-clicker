namespace MyHeroesClicker.Core.Contracts.Database;

public sealed record EquipmentSetResponse(
  long Id,
  long? AccountId,
  string Kind);

public sealed record EquipmentSetSlotResponse(
  long EquipmentSetId,
  int SlotNumber,
  string ExpectedImageSrc,
  bool ShouldBeEmpty);

public sealed record CreateEquipmentSetRequest(
  long? AccountId,
  string Kind);

public sealed record UpdateEquipmentSetRequest(
  long? AccountId,
  string Kind);

public sealed record UpsertEquipmentSetSlotRequest(
  string ExpectedImageSrc,
  bool ShouldBeEmpty);
