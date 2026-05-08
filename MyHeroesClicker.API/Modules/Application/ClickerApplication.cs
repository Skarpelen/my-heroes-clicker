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

  public string? LastError { get; private set; }

  public Task StartFarmAsync(int targetIterations, CancellationToken cancellationToken)
  {
    ConfigureRun(targetIterations);

    return StartScenarioAsync(_runtime.Scenarios.FarmCycle, cancellationToken);
  }

  public Task StartAdventureFarmAsync(int targetIterations, CancellationToken cancellationToken)
  {
    ConfigureRun(targetIterations);

    return StartScenarioAsync(_runtime.Scenarios.AdventureFarmCycle, cancellationToken);
  }

  public Task StartFarmPreparationAsync(CancellationToken cancellationToken)
  {
    return StartScenarioAsync(_runtime.Scenarios.FarmPreparation, cancellationToken);
  }

  public Task StartCombatPreparationAsync(CancellationToken cancellationToken)
  {
    return StartScenarioAsync(_runtime.Scenarios.CombatPreparation, cancellationToken);
  }

  public Task PrepareFarmAsync(CancellationToken cancellationToken)
  {
    return RunScenarioAsync(_runtime.Scenarios.FarmPreparation, cancellationToken);
  }

  public Task PrepareCombatAsync(CancellationToken cancellationToken)
  {
    return RunScenarioAsync(_runtime.Scenarios.CombatPreparation, cancellationToken);
  }

  public void StopCurrentScenario()
  {
    lock (_sync)
    {
      _currentScenarioCancellation?.Cancel();
    }
  }

  private void ConfigureRun(int targetIterations)
  {
    if (targetIterations <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(targetIterations), "Количество атак должно быть положительным.");
    }

    _runtime.Context.ResetIterations(targetIterations);
  }

  private Task StartScenarioAsync(IScenario scenario, CancellationToken cancellationToken)
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

  private async Task RunScenarioAsync(IScenario scenario, CancellationToken cancellationToken)
  {
    await StartScenarioAsync(scenario, cancellationToken);

    Task scenarioTask;

    lock (_sync)
    {
      scenarioTask = _currentScenarioTask ?? Task.CompletedTask;
    }

    await scenarioTask;
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
