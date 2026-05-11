namespace MyHeroesClicker.Core.Models.Scenario;

public sealed class ScenarioStatusResponse
{
  public ScenarioStatusResponse(
    bool isInitialized,
    bool isRunning,
    bool isPaused,
    string? activeScenarioKey,
    string? activeScenarioName,
    string? browserTabName,
    IReadOnlyCollection<string> runningScenarioKeys,
    string? pauseReason,
    DateTimeOffset? pauseRequestedAt,
    string? lastUserEvent,
    DateTimeOffset? lastUserEventAt,
    int? iterationLimit,
    int? completedIterations,
    int? maxHealth,
    string? warState,
    DateTimeOffset? warNextCheckAt,
    string? lastError)
  {
    IsInitialized = isInitialized;
    IsRunning = isRunning;
    IsPaused = isPaused;
    ActiveScenarioKey = activeScenarioKey;
    ActiveScenarioName = activeScenarioName;
    BrowserTabName = browserTabName;
    RunningScenarioKeys = runningScenarioKeys;
    PauseReason = pauseReason;
    PauseRequestedAt = pauseRequestedAt;
    LastUserEvent = lastUserEvent;
    LastUserEventAt = lastUserEventAt;
    IterationLimit = iterationLimit;
    CompletedIterations = completedIterations;
    MaxHealth = maxHealth;
    WarState = warState;
    WarNextCheckAt = warNextCheckAt;
    LastError = lastError;
  }

  public bool IsInitialized { get; }

  public bool IsRunning { get; }

  public bool IsPaused { get; }

  public string? ActiveScenarioKey { get; }

  public string? ActiveScenarioName { get; }

  public string? BrowserTabName { get; }

  public IReadOnlyCollection<string> RunningScenarioKeys { get; }

  public string? PauseReason { get; }

  public DateTimeOffset? PauseRequestedAt { get; }

  public string? LastUserEvent { get; }

  public DateTimeOffset? LastUserEventAt { get; }

  public int? IterationLimit { get; }

  public int? CompletedIterations { get; }

  public int? MaxHealth { get; }

  public string? WarState { get; }

  public DateTimeOffset? WarNextCheckAt { get; }

  public string? LastError { get; }
}
