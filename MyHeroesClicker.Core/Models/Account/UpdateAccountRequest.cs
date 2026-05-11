namespace MyHeroesClicker.Core.Models.Account;

public sealed class UpdateAccountRequest
{
  public UpdateAccountRequest()
  {
  }

  public UpdateAccountRequest(string login, string? password, bool isEnabled)
  {
    Login = login;
    Password = password;
    IsEnabled = isEnabled;
  }

  public string Login { get; init; } = string.Empty;

  public string? Password { get; init; }

  public bool IsEnabled { get; init; }
}
