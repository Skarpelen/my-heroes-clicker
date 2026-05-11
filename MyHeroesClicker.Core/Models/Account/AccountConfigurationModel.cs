namespace MyHeroesClicker.Core.Models.Account;

/// <summary>
/// Базовая модель привязанной к аккаунту конфигурации.
/// </summary>
public abstract class AccountConfigurationModel
{
  /// <summary>
  /// Идентификатор конфигурации.
  /// </summary>
  public long Id { get; init; }

  /// <summary>
  /// Идентификатор аккаунта или null для глобальной конфигурации.
  /// </summary>
  public long? AccountId { get; init; }

  /// <summary>
  /// Тип конфигурации.
  /// </summary>
  public string Kind { get; init; } = string.Empty;
}