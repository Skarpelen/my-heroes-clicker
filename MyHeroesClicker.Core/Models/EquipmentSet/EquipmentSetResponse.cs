namespace MyHeroesClicker.Core.Models.EquipmentSet;

public sealed class EquipmentSetResponse
{
  public EquipmentSetResponse(long id, long? accountId, string kind)
  {
    Id = id;
    AccountId = accountId;
    Kind = kind;
  }

  public long Id { get; }

  public long? AccountId { get; }

  public string Kind { get; }
}
