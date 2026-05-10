namespace MyHeroesClicker.Core.Contracts;

public sealed record ScenarioStatusResponse(
  bool IsInitialized,
  bool IsRunning,
  bool IsPaused,
  string? ActiveScenarioKey,
  string? ActiveScenarioName,
  string? BrowserTabName,
  IReadOnlyCollection<string> RunningScenarioKeys,
  string? PauseReason,
  DateTimeOffset? PauseRequestedAt,
  string? LastUserEvent,
  DateTimeOffset? LastUserEventAt,
  int? IterationLimit,
  int? CompletedIterations,
  int? MaxHealth,
  string? WarState,
  DateTimeOffset? WarNextCheckAt,
  string? LastError);
