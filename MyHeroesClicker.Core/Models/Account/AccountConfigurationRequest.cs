namespace MyHeroesClicker.Core.Models.Account;

/// <summary>
/// Базовый запрос привязанной к аккаунту конфигурации.
/// </summary>
public abstract class AccountConfigurationRequest
{
  /// <summary>
  /// Идентификатор аккаунта или null для глобальной конфигурации.
  /// </summary>
  public long? AccountId { get; init; }

  /// <summary>
  /// Тип конфигурации.
  /// </summary>
  public string Kind { get; init; } = string.Empty;
}