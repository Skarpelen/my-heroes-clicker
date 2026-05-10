namespace MyHeroesClicker.Core.Confs;

public sealed record ScenarioConfiguration(
  EquipmentModeConfiguration Equipment,
  TechniqueModeConfiguration Techniques,
  WarModeConfiguration War);

public sealed record EquipmentModeConfiguration(
  IReadOnlyCollection<EquipmentSlotConfiguration> FarmSlots,
  IReadOnlyCollection<EquipmentSlotConfiguration> CombatSlots);

public sealed record EquipmentSlotConfiguration(
  int SlotNumber,
  int? ItemId,
  bool ShouldBeEmpty);

public sealed record TechniqueModeConfiguration(
  IReadOnlySet<int> FarmEnabledTechniqueIds,
  IReadOnlySet<int> CombatEnabledTechniqueIds);

public sealed record WarModeConfiguration(
  int CheckIntervalMinutes,
  int CombatPreparationSecondsBeforeRegistrationEnd);
