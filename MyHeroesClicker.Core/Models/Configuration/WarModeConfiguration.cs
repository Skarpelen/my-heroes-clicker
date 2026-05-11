namespace MyHeroesClicker.Core.Models.Configuration;

/// <summary>
/// Конфигурация сценария войны.
/// </summary>
public sealed class WarModeConfiguration
{
  /// <summary>
  /// Интервал проверки войны в минутах.
  /// </summary>
  public int CheckIntervalMinutes { get; init; }

  /// <summary>
  /// Смещение подготовки к бою до окончания регистрации в секундах.
  /// </summary>
  public int CombatPreparationSecondsBeforeRegistrationEnd { get; init; }
}