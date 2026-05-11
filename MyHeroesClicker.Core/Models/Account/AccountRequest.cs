namespace MyHeroesClicker.Core.Models.Account;

/// <summary>
/// Базовый запрос аккаунта с общими полями для операций создания и обновления.
/// </summary>
public abstract class AccountRequest
{
  /// <summary>
  /// Логин аккаунта.
  /// </summary>
  public string Login { get; init; } = string.Empty;

  /// <summary>
  /// Пароль аккаунта.
  /// </summary>
  public string? Password { get; init; }

  /// <summary>
  /// Включен ли аккаунт.
  /// </summary>
  public bool IsEnabled { get; init; }
}