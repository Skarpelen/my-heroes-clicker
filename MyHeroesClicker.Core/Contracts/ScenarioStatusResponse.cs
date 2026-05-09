namespace MyHeroesClicker.Core.Contracts;

public sealed record ScenarioStatusResponse(
  bool IsInitialized,
  bool IsRunning,
  bool IsPaused,
  string? ActiveScenarioKey,
  string? ActiveScenarioName,
  string? BrowserTabName,
  string? PauseReason,
  DateTimeOffset? PauseRequestedAt,
  string? LastUserEvent,
  DateTimeOffset? LastUserEventAt,
  int? TargetIterations,
  int? CompletedIterations,
  int? MaxHealth,
  string? LastError);
