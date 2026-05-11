namespace MyHeroesClicker.Core.Models.TechniquePreset;

public sealed class TechniquePresetSlotResponse
{
  public TechniquePresetSlotResponse(
    long techniquePresetId,
    int techniqueNumber,
    string techniqueName,
    bool isEnabled)
  {
    TechniquePresetId = techniquePresetId;
    TechniqueNumber = techniqueNumber;
    TechniqueName = techniqueName;
    IsEnabled = isEnabled;
  }

  public long TechniquePresetId { get; }

  public int TechniqueNumber { get; }

  public string TechniqueName { get; }

  public bool IsEnabled { get; }
}
