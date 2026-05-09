namespace MyHeroesClicker.Core.Contracts.Database;

public sealed record TechniquePresetResponse(
  long Id,
  long? AccountId,
  string Kind);

public sealed record TechniquePresetSlotResponse(
  long TechniquePresetId,
  int TechniqueNumber,
  string TechniqueName,
  bool IsEnabled);

public sealed record CreateTechniquePresetRequest(
  long? AccountId,
  string Kind);

public sealed record UpdateTechniquePresetRequest(
  long? AccountId,
  string Kind);

public sealed record UpsertTechniquePresetSlotRequest(
  string TechniqueName,
  bool IsEnabled);
