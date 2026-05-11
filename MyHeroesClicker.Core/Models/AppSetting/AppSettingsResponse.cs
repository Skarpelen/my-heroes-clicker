namespace MyHeroesClicker.Core.Models.AppSetting;

public sealed class AppSettingsResponse
{
  public AppSettingsResponse(
    long id,
    long? activeAccountId,
    string baseUrl,
    string browserKind,
    bool headless,
    string? userDataDir,
    int minDelayMs,
    int maxDelayMs,
    int defaultTimeoutMs,
    int hpRecoveryDelayMultiplier,
    double minAttackHealthPercent,
    double maxAttackHealthPercent,
    int maxStepRetryCount,
    int retryDelayMs,
    int authenticationRetryDelayMs,
    int warCheckIntervalMinutes,
    int warCombatPreparationSecondsBeforeRegistrationEnd)
  {
    Id = id;
    ActiveAccountId = activeAccountId;
    BaseUrl = baseUrl;
    BrowserKind = browserKind;
    Headless = headless;
    UserDataDir = userDataDir;
    MinDelayMs = minDelayMs;
    MaxDelayMs = maxDelayMs;
    DefaultTimeoutMs = defaultTimeoutMs;
    HpRecoveryDelayMultiplier = hpRecoveryDelayMultiplier;
    MinAttackHealthPercent = minAttackHealthPercent;
    MaxAttackHealthPercent = maxAttackHealthPercent;
    MaxStepRetryCount = maxStepRetryCount;
    RetryDelayMs = retryDelayMs;
    AuthenticationRetryDelayMs = authenticationRetryDelayMs;
    WarCheckIntervalMinutes = warCheckIntervalMinutes;
    WarCombatPreparationSecondsBeforeRegistrationEnd = warCombatPreparationSecondsBeforeRegistrationEnd;
  }

  public long Id { get; }

  public long? ActiveAccountId { get; }

  public string BaseUrl { get; }

  public string BrowserKind { get; }

  public bool Headless { get; }

  public string? UserDataDir { get; }

  public int MinDelayMs { get; }

  public int MaxDelayMs { get; }

  public int DefaultTimeoutMs { get; }

  public int HpRecoveryDelayMultiplier { get; }

  public double MinAttackHealthPercent { get; }

  public double MaxAttackHealthPercent { get; }

  public int MaxStepRetryCount { get; }

  public int RetryDelayMs { get; }

  public int AuthenticationRetryDelayMs { get; }

  public int WarCheckIntervalMinutes { get; }

  public int WarCombatPreparationSecondsBeforeRegistrationEnd { get; }
}
