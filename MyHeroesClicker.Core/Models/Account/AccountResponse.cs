namespace MyHeroesClicker.Core.Models.Account;

public sealed class AccountResponse
{
  public AccountResponse(long id, string login, bool hasPassword, bool isEnabled)
  {
    Id = id;
    Login = login;
    HasPassword = hasPassword;
    IsEnabled = isEnabled;
  }

  public long Id { get; }

  public string Login { get; }

  public bool HasPassword { get; }

  public bool IsEnabled { get; }
}
