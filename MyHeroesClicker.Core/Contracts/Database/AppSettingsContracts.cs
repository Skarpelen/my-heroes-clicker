namespace MyHeroesClicker.Core.Contracts.Database;

public sealed record AppSettingsResponse(
  long Id,
  long? ActiveAccountId,
  string BaseUrl,
  bool Headless,
  string? UserDataDir,
  int MinDelayMs,
  int MaxDelayMs,
  int DefaultTimeoutMs,
  int HpRecoveryDelayMultiplier,
  double MinAttackHealthPercent,
  double MaxAttackHealthPercent,
  int MaxStepRetryCount,
  int RetryDelayMs,
  int AuthenticationRetryDelayMs);

public sealed record UpdateAppSettingsRequest(
  string BaseUrl,
  bool Headless,
  string? UserDataDir,
  int MinDelayMs,
  int MaxDelayMs,
  int DefaultTimeoutMs,
  int HpRecoveryDelayMultiplier,
  double MinAttackHealthPercent,
  double MaxAttackHealthPercent,
  int MaxStepRetryCount,
  int RetryDelayMs,
  int AuthenticationRetryDelayMs);

public sealed record SetActiveAccountRequest(long? AccountId);
