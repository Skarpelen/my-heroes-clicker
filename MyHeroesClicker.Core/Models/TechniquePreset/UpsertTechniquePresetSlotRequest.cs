namespace MyHeroesClicker.Core.Models.TechniquePreset;

public sealed class UpsertTechniquePresetSlotRequest
{
  public UpsertTechniquePresetSlotRequest()
  {
  }

  public UpsertTechniquePresetSlotRequest(string techniqueName, bool isEnabled)
  {
    TechniqueName = techniqueName;
    IsEnabled = isEnabled;
  }

  public string TechniqueName { get; init; } = string.Empty;

  public bool IsEnabled { get; init; }
}
