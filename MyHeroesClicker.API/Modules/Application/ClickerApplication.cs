using MyHeroesClicker.API.Modules.Runtime;
using MyHeroesClicker.Core.Models.Scenarios;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.API.Modules.Application;

public sealed class ClickerApplication : IDisposable
{
  private readonly ClickerRuntime _runtime;
  private readonly ScenarioDispatcher _dispatcher = new();
  private readonly Dictionary<ScenarioBrowserTabKind, ScenarioRunState> _runningScenarios = new();
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

  public int TargetIterations => GetStatusContext().TargetIterations;

  public int CompletedIterations => GetStatusContext().CompletedIterations;

  public string? ActiveScenarioKey => GetActiveRun()?.Entry.Key;

  public string? ActiveScenarioName => GetActiveRun()?.Entry.Scenario.Name;

  public string? BrowserTabName => GetActiveRun()?.Entry.BrowserTabName ?? GetStatusContext().BrowserTabName;

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
        run.Cancellation.Cancel();
      }
    }
  }

  public async Task StartScenarioAsync(
    ScenarioCatalogEntry entry,
    int targetIterations,
    CancellationToken cancellationToken)
  {
    if (targetIterations <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(targetIterations), "Количество атак должно быть положительным.");
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
      context = await _runtime.CreateContextAsync(entry, targetIterations, cancellationToken);
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
    catch (Exception exception)
    {
      LastError = exception.Message;
      context.Logger.Error(exception.Message);
      throw;
    }
    finally
    {
      lock (_sync)
      {
        if (_runningScenarios.Remove(entry.BrowserTabKind, out var state))
        {
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

internal sealed class ScenarioRunState
{
  public ScenarioRunState(
    ScenarioCatalogEntry entry,
    ScenarioContext context,
    CancellationTokenSource cancellation)
  {
    Entry = entry;
    Context = context;
    Cancellation = cancellation;
  }

  public ScenarioCatalogEntry Entry { get; }

  public ScenarioContext Context { get; }

  public CancellationTokenSource Cancellation { get; }

  public Task Task { get; set; } = Task.CompletedTask;
}
