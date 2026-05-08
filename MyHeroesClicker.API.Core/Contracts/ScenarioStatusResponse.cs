namespace MyHeroesClicker.API.Contracts;

public sealed record ScenarioStatusResponse(
  bool IsInitialized,
  bool IsRunning,
  bool IsPaused,
  string? PauseReason,
  int? TargetIterations,
  int? CompletedIterations,
  int? MaxHealth,
  string? LastError);
