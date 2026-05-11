namespace MyHeroesClicker.Core.Models.Account;

/// <summary>
/// Данные аккаунта, возвращаемые клиентам API.
/// </summary>
public sealed class AccountResponse : AccountModel
{
  /// <summary>
  /// Сохранен ли пароль для аккаунта.
  /// </summary>
  public bool HasPassword { get; init; }
}