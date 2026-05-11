namespace MyHeroesClicker.Core.Models.Configuration;

public sealed class WarModeConfiguration
{
  public WarModeConfiguration(
    int checkIntervalMinutes,
    int combatPreparationSecondsBeforeRegistrationEnd)
  {
    CheckIntervalMinutes = checkIntervalMinutes;
    CombatPreparationSecondsBeforeRegistrationEnd = combatPreparationSecondsBeforeRegistrationEnd;
  }

  public int CheckIntervalMinutes { get; }

  public int CombatPreparationSecondsBeforeRegistrationEnd { get; }
}
