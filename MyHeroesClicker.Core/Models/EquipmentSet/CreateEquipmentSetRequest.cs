namespace MyHeroesClicker.Core.Models.EquipmentSet;

public sealed class CreateEquipmentSetRequest
{
  public CreateEquipmentSetRequest()
  {
  }

  public CreateEquipmentSetRequest(long? accountId, string kind)
  {
    AccountId = accountId;
    Kind = kind;
  }

  public long? AccountId { get; init; }

  public string Kind { get; init; } = string.Empty;
}
