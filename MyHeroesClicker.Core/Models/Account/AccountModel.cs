namespace MyHeroesClicker.Core.Models.Account;

/// <summary>
/// Базовая модель аккаунта с общими полями для ответов аккаунта.
/// </summary>
public abstract class AccountModel
{
  /// <summary>
  /// Идентификатор аккаунта.
  /// </summary>
  public long Id { get; init; }

  /// <summary>
  /// Логин аккаунта.
  /// </summary>
  public string Login { get; init; } = string.Empty;

  /// <summary>
  /// Включен ли аккаунт.
  /// </summary>
  public bool IsEnabled { get; init; }
}