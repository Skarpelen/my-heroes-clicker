namespace MyHeroesClicker.Core.Models.AppSetting;

public sealed class UpdateAppSettingsRequest
{
  public string BaseUrl { get; init; } = string.Empty;

  public string BrowserKind { get; init; } = string.Empty;

  public bool Headless { get; init; }

  public string? UserDataDir { get; init; }

  public int MinDelayMs { get; init; }

  public int MaxDelayMs { get; init; }

  public int DefaultTimeoutMs { get; init; }

  public int HpRecoveryDelayMultiplier { get; init; }

  public double MinAttackHealthPercent { get; init; }

  public double MaxAttackHealthPercent { get; init; }

  public int MaxStepRetryCount { get; init; }

  public int RetryDelayMs { get; init; }

  public int AuthenticationRetryDelayMs { get; init; }

  public int WarCheckIntervalMinutes { get; init; }

  public int WarCombatPreparationSecondsBeforeRegistrationEnd { get; init; }
}
