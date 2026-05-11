namespace MyHeroesClicker.Core.Models.Account;

public sealed class AccountCredentialsResponse
{
  public AccountCredentialsResponse(long id, string login, string? password, bool isEnabled)
  {
    Id = id;
    Login = login;
    Password = password;
    IsEnabled = isEnabled;
  }

  public long Id { get; }

  public string Login { get; }

  public string? Password { get; }

  public bool IsEnabled { get; }
}
