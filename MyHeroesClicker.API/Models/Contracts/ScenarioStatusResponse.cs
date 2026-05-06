namespace MyHeroesClicker.API.Contracts;

public sealed record ScenarioStatusResponse(
  bool IsInitialized,
  bool IsRunning,
  int? TargetIterations,
  int? CompletedIterations,
  int? MaxHealth,
  string? LastError);
