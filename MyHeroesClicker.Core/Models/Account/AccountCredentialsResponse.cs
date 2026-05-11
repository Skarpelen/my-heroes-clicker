namespace MyHeroesClicker.Core.Models.Account;

/// <summary>
/// Данные аккаунта с расшифрованными учетными данными для внутреннего использования во время выполнения.
/// </summary>
public sealed class AccountCredentialsResponse : AccountModel
{
  /// <summary>
  /// Расшифрованный пароль аккаунта.
  /// </summary>
  public string? Password { get; init; }
}