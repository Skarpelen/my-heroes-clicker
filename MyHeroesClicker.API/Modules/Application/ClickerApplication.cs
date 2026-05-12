using MyHeroesClicker.API.Modules.Runtime;
using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Models.Scenario;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.API.Modules.Application;

public sealed class ClickerApplication : IScenarioCoordinator, IDisposable
{
  private readonly ClickerRuntime _runtime;
  private readonly ScenarioDispatcher _dispatcher = new();
  private readonly Dictionary<ScenarioBrowserTabKind, ScenarioRunState> _runningScenarios = new();
  private readonly List<ScenarioRunState> _recentScenarioRuns = new();
  private readonly object _sync = new();
  private ScenarioContext _lastContext;

  public ClickerApplication(ClickerRuntime runtime)
  {
    _runtime = runtime;
    _lastContext = runtime.Context;
  }

  public bool IsRunning
  {
    get
    {
      lock (_sync)
      {
        return _runningScenarios.Values.Any(run => !run.Task.IsCompleted);
      }
    }
  }

  public int? IterationLimit => GetStatusContext().RunOptions.IterationLimit;

  public int CompletedIterations => GetStatusContext().CompletedIterations;

  public string? ActiveScenarioKey => GetActiveRun()?.Entry.Key;

  public string? ActiveScenarioName => GetActiveRun()?.Entry.Scenario.Name;

  public string? BrowserTabName => GetActiveRun()?.Entry.BrowserTabName ?? GetStatusContext().BrowserTabName;

  public IReadOnlyCollection<string> RunningScenarioKeys
  {
    get
    {
      lock (_sync)
      {
        return _runningScenarios.Values
          .Where(run => !run.Task.IsCompleted)
          .Select(run => run.Entry.Key)
          .ToArray();
      }
    }
  }

  public IReadOnlyCollection<ScenarioRunStatusResponse> ScenarioRuns
  {
    get
    {
      lock (_sync)
      {
        return _runningScenarios.Values
          .Where(run => !run.Task.IsCompleted)
          .Concat(_recentScenarioRuns)
          .Select(CreateRunStatus)
          .ToArray();
      }
    }
  }

  public string? WarState => GetRunningContext(ScenarioBrowserTabKind.ClanWar)?.StatusMessage;

  public DateTimeOffset? WarNextCheckAt => GetRunningContext(ScenarioBrowserTabKind.ClanWar)?.NextCheckAt;

  public int? MaxHealth
  {
    get
    {
      var context = GetStatusContext();

      return context.CharacterState.MaxHealth > 0
        ? context.CharacterState.MaxHealth
        : null;
    }
  }

  public ScenarioCatalog Scenarios => _runtime.Scenarios;

  public string? LastError { get; private set; }

  public void StopCurrentScenario()
  {
    lock (_sync)
    {
      foreach (var run in _runningScenarios.Values)
      {
        run.RequestStop();
      }
    }
  }

  public bool StopScenario(string scenarioKey)
  {
    lock (_sync)
    {
      var runs = _runningScenarios.Values
        .Where(run => run.Entry.Key == scenarioKey && !run.Task.IsCompleted)
        .ToArray();

      foreach (var run in runs)
      {
        run.RequestStop();
      }

      return runs.Length > 0;
    }
  }

  public async Task StartScenarioAsync(
    ScenarioCatalogEntry entry,
    ScenarioRunOptions runOptions,
    CancellationToken cancellationToken)
  {
    if (runOptions.IterationLimit is <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(runOptions), "Количество атак должно быть положительным.");
    }

    ScenarioDispatchLease lease;

    lock (_sync)
    {
      if (_runningScenarios.ContainsKey(entry.BrowserTabKind))
      {
        throw new InvalidOperationException($"Вкладка \"{entry.BrowserTabName}\" уже занята другим сценарием.");
      }

      lease = _dispatcher.TryAcquire(entry);
    }

    ScenarioContext context;

    try
    {
      context = await _runtime.CreateContextAsync(entry, runOptions, this, cancellationToken);
    }
    catch
    {
      lease.Dispose();
      throw;
    }

    var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    var state = new ScenarioRunState(entry, context, linkedCancellation);

    lock (_sync)
    {
      _runningScenarios.Add(entry.BrowserTabKind, state);
      _lastContext = context;
    }

    state.Task = ExecuteScenarioAsync(entry, context, linkedCancellation.Token, lease);
  }

  public void Dispose()
  {
    _dispatcher.Dispose();
  }

  public async Task StopGroupAsync(ScenarioConcurrencyGroup group, CancellationToken cancellationToken)
  {
    ScenarioRunState[] runs;

    lock (_sync)
    {
      runs = _runningScenarios.Values
        .Where(run => run.Entry.ConcurrencyGroup == group && !run.Task.IsCompleted)
        .ToArray();

      foreach (var run in runs)
      {
        run.RequestStop();
      }
    }

    foreach (var run in runs)
    {
      try
      {
        await run.Task.WaitAsync(cancellationToken);
      }
      catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
      {
      }
    }
  }

  private async Task ExecuteScenarioAsync(
    ScenarioCatalogEntry entry,
    ScenarioContext context,
    CancellationToken cancellationToken,
    ScenarioDispatchLease lease)
  {
    try
    {
      LastError = null;
      context.Logger.Log($"Выполняется сценарий: {entry.Scenario.Name} ({FormatExecutionMode(entry.ExecutionMode)}).");
      await entry.Scenario.ExecuteAsync(context, cancellationToken);
    }
    catch (OperationCanceledException)
    {
      context.Logger.Warn($"Сценарий остановлен: {entry.Scenario.Name}.");
    }
    catch (Exception exception)
    {
      LastError = exception.Message;
      var state = GetRunningState(entry.BrowserTabKind);

      if (state is not null)
      {
        state.LastError = exception.Message;
      }

      context.Logger.Error(exception.Message);
      throw;
    }
    finally
    {
      lock (_sync)
      {
        if (_runningScenarios.Remove(entry.BrowserTabKind, out var state))
        {
          AddRecentScenarioRun(state);
          state.Cancellation.Dispose();
        }

        _lastContext = context;
      }

      lease.Dispose();
    }
  }

  private ScenarioContext GetStatusContext()
  {
    lock (_sync)
    {
      return _runningScenarios.Values.FirstOrDefault()?.Context ?? _lastContext;
    }
  }

  private ScenarioRunState? GetActiveRun()
  {
    lock (_sync)
    {
      return _runningScenarios.Values.FirstOrDefault(run => !run.Task.IsCompleted);
    }
  }

  private ScenarioContext? GetRunningContext(ScenarioBrowserTabKind tabKind)
  {
    lock (_sync)
    {
      return _runningScenarios.TryGetValue(tabKind, out var run) && !run.Task.IsCompleted
        ? run.Context
        : null;
    }
  }

  private ScenarioRunState? GetRunningState(ScenarioBrowserTabKind tabKind)
  {
    lock (_sync)
    {
      return _runningScenarios.TryGetValue(tabKind, out var run) && !run.Task.IsCompleted
        ? run
        : null;
    }
  }

  private static ScenarioRunStatusResponse CreateRunStatus(ScenarioRunState run)
  {
    var iterationLimit = run.Context.RunOptions.IterationLimit;
    var completedIterations = run.Context.CompletedIterations;

    return new ScenarioRunStatusResponse
    {
      RunId = run.RunId.ToString("N"),
      ScenarioKey = run.Entry.Key,
      ScenarioName = run.Entry.Scenario.Name,
      BrowserTabKind = run.Entry.BrowserTabKind.ToString(),
      BrowserTabName = run.Entry.BrowserTabName,
      State = GetRunState(run),
      StartedAt = run.StartedAt,
      StopRequestedAt = run.StopRequestedAt,
      IterationLimit = iterationLimit,
      CompletedIterations = completedIterations,
      ProgressPercent = CalculateProgressPercent(completedIterations, iterationLimit),
      StatusMessage = run.Context.StatusMessage,
      NextCheckAt = run.Context.NextCheckAt,
      LastError = run.LastError
    };
  }

  private static string GetRunState(ScenarioRunState run)
  {
    if (run.Task.IsFaulted)
    {
      return "failed";
    }

    if (run.Task.IsCompleted)
    {
      return "stopped";
    }

    if (run.StopRequestedAt is not null)
    {
      return "stopping";
    }

    if (run.Context.PauseService.IsPauseRequested)
    {
      return "paused";
    }

    return "running";
  }

  private void AddRecentScenarioRun(ScenarioRunState run)
  {
    _recentScenarioRuns.Insert(0, run);

    if (_recentScenarioRuns.Count > 10)
    {
      _recentScenarioRuns.RemoveRange(10, _recentScenarioRuns.Count - 10);
    }
  }

  private static int? CalculateProgressPercent(int completedIterations, int? iterationLimit)
  {
    if (iterationLimit is null or <= 0)
    {
      return null;
    }

    return Math.Min(100, (int)Math.Round((double)completedIterations / iterationLimit.Value * 100));
  }

  private static string FormatExecutionMode(ScenarioExecutionMode mode)
  {
    return mode switch
    {
      ScenarioExecutionMode.Http => "HTTP",
      ScenarioExecutionMode.Playwright => "Playwright",
      ScenarioExecutionMode.Mixed => "смешанный",
      _ => mode.ToString()
    };
  }
}
