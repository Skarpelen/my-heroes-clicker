namespace MyHeroesClicker.Core.Models.Configuration;

public sealed class ScenarioConfiguration
{
  public ScenarioConfiguration(
    EquipmentModeConfiguration equipment,
    TechniqueModeConfiguration techniques,
    WarModeConfiguration war)
  {
    Equipment = equipment;
    Techniques = techniques;
    War = war;
  }

  public EquipmentModeConfiguration Equipment { get; }

  public TechniqueModeConfiguration Techniques { get; }

  public WarModeConfiguration War { get; }
}
