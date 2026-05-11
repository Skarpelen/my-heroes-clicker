namespace MyHeroesClicker.Core.Models.Configuration;

public sealed class TechniqueModeConfiguration
{
  public TechniqueModeConfiguration(
    IReadOnlySet<int> farmEnabledTechniqueIds,
    IReadOnlySet<int> combatEnabledTechniqueIds)
  {
    FarmEnabledTechniqueIds = farmEnabledTechniqueIds;
    CombatEnabledTechniqueIds = combatEnabledTechniqueIds;
  }

  public IReadOnlySet<int> FarmEnabledTechniqueIds { get; }

  public IReadOnlySet<int> CombatEnabledTechniqueIds { get; }
}
