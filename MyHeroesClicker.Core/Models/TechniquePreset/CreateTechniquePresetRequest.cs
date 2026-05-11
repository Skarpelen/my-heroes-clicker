namespace MyHeroesClicker.Core.Models.TechniquePreset;

public sealed class CreateTechniquePresetRequest
{
  public CreateTechniquePresetRequest()
  {
  }

  public CreateTechniquePresetRequest(long? accountId, string kind)
  {
    AccountId = accountId;
    Kind = kind;
  }

  public long? AccountId { get; init; }

  public string Kind { get; init; } = string.Empty;
}
