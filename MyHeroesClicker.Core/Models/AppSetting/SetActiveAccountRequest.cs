namespace MyHeroesClicker.Core.Models.AppSetting;

/// <summary>
/// Запрос на установку активного аккаунта.
/// </summary>
public sealed class SetActiveAccountRequest
{
  /// <summary>
  /// Идентификатор активного аккаунта.
  /// </summary>
  public long? AccountId { get; init; }
}