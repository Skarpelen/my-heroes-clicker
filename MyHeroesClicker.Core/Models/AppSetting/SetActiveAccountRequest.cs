namespace MyHeroesClicker.Core.Models.AppSetting;

public sealed class SetActiveAccountRequest
{
  public SetActiveAccountRequest()
  {
  }

  public SetActiveAccountRequest(long? accountId)
  {
    AccountId = accountId;
  }

  public long? AccountId { get; init; }
}
