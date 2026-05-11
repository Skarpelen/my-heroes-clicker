namespace MyHeroesClicker.Core.Models.TechniquePreset;

public sealed class TechniquePresetResponse
{
  public TechniquePresetResponse(long id, long? accountId, string kind)
  {
    Id = id;
    AccountId = accountId;
    Kind = kind;
  }

  public long Id { get; }

  public long? AccountId { get; }

  public string Kind { get; }
}
