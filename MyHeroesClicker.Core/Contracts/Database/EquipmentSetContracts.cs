namespace MyHeroesClicker.API.Contracts.Database;

public sealed record EquipmentSetResponse(
  long Id,
  long? AccountId,
  string Kind,
  string Name);

public sealed record EquipmentSetSlotResponse(
  long EquipmentSetId,
  int SlotNumber,
  string ExpectedImageSrc,
  bool ShouldBeEmpty);

public sealed record CreateEquipmentSetRequest(
  long? AccountId,
  string Kind,
  string Name);

public sealed record UpdateEquipmentSetRequest(
  long? AccountId,
  string Kind,
  string Name);

public sealed record UpsertEquipmentSetSlotRequest(
  string ExpectedImageSrc,
  bool ShouldBeEmpty);
