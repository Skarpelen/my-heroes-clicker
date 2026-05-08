using MyHeroesClicker.Core;
using MyHeroesClicker.Runtime;

namespace MyHeroesClicker.Application;

public sealed class ClickerApplication
{
  private readonly ClickerRuntime _runtime;
  private readonly object _sync = new();
  private CancellationTokenSource? _currentScenarioCancellation;
  private Task? _currentScenarioTask;

  public ClickerApplication(ClickerRuntime runtime)
  {
    _runtime = runtime;
  }

  public bool IsRunning
  {
    get
    {
      lock (_sync)
      {
        return _currentScenarioTask is { IsCompleted: false };
      }
    }
  }

  public int TargetIterations => _runtime.Context.TargetIterations;

  public int CompletedIterations => _runtime.Context.CompletedIterations;

  public int? MaxHealth => _runtime.Context.CharacterState.MaxHealth > 0
    ? _runtime.Context.CharacterState.MaxHealth
    : null;

  public ScenarioCatalog Scenarios => _runtime.Scenarios;

  public string? LastError { get; private set; }

  public void StopCurrentScenario()
  {
    lock (_sync)
    {
      _currentScenarioCancellation?.Cancel();
    }
  }

  public void ConfigureRun(int targetIterations)
  {
    if (targetIterations <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(targetIterations), "Количество атак должно быть положительным.");
    }

    _runtime.Context.ResetIterations(targetIterations);
  }

  public Task StartScenarioAsync(IScenario scenario, CancellationToken cancellationToken)
  {
    lock (_sync)
    {
      if (_currentScenarioTask is { IsCompleted: false })
      {
        throw new InvalidOperationException("Сценарий уже выполняется.");
      }

      _currentScenarioCancellation?.Dispose();
      _currentScenarioCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      _currentScenarioTask = ExecuteCurrentScenarioAsync(scenario, _currentScenarioCancellation.Token);

      return Task.CompletedTask;
    }
  }

  private async Task ExecuteCurrentScenarioAsync(IScenario scenario, CancellationToken cancellationToken)
  {
    try
    {
      LastError = null;
      _runtime.Context.Logger.Log($"Выполняется сценарий: {scenario.Name}");
      await scenario.ExecuteAsync(_runtime.Context, cancellationToken);
    }
    catch (Exception exception)
    {
      LastError = exception.Message;
      _runtime.Context.Logger.Error(exception.Message);
      throw;
    }
    finally
    {
      lock (_sync)
      {
        _currentScenarioCancellation?.Dispose();
        _currentScenarioCancellation = null;
        _currentScenarioTask = null;
      }
    }
  }
}
