using MyHeroesClicker.API.Modules.Application;
using MyHeroesClicker.API.Modules.Runtime;
using MyHeroesClicker.Core.Confs;
using MyHeroesClicker.Core.Contracts;
using MyHeroesClicker.Core.Interfaces.Services;

namespace MyHeroesClicker.API.Modules.Services;

public sealed class ClickerApiService : IAsyncDisposable
{
  private readonly SemaphoreSlim _initializationSync = new(1, 1);
  private readonly ClickerOptions _options;
  private readonly IRunLogger _logger;
  private readonly IAlertService _alertService;
  private readonly IPauseService _pauseService;
  private ClickerRuntime? _runtime;
  private ClickerApplication? _application;
  private string? _lastError;

  public ClickerApiService(
    ClickerOptions options,
    IRunLogger logger,
    IAlertService alertService,
    IPauseService pauseService)
  {
    _options = options;
    _logger = logger;
    _alertService = alertService;
    _pauseService = pauseService;
  }

  public ScenarioStatusResponse GetStatus()
  {
    if (_application is null)
    {
      return new ScenarioStatusResponse(
        false,
        false,
        _pauseService.IsPauseRequested,
        _pauseService.PauseReason,
        null,
        null,
        null,
        _lastError);
    }

    return new ScenarioStatusResponse(
      true,
      _application.IsRunning,
      _pauseService.IsPauseRequested,
      _pauseService.PauseReason,
      _application.TargetIterations,
      _application.CompletedIterations,
      _application.MaxHealth,
      _application.LastError ?? _lastError);
  }

  public async Task<ClickerApplication> GetApplicationAsync(
    int initialTargetIterations,
    CancellationToken cancellationToken)
  {
    if (_application is not null)
    {
      return _application;
    }

    await _initializationSync.WaitAsync(cancellationToken);

    try
    {
      if (_application is not null)
      {
        return _application;
      }

      var config = ClickerConfigLoader.Load(_options, _logger);
      _runtime = await ClickerRuntime.StartAsync(
        _options,
        config,
        _logger,
        _alertService,
        _pauseService,
        initialTargetIterations);

      _application = new ClickerApplication(_runtime);

      return _application;
    }
    finally
    {
      _initializationSync.Release();
    }
  }

  public void ResetPause()
  {
    _pauseService.Reset();
  }

  public void Stop()
  {
    _pauseService.Reset();
    _application?.StopCurrentScenario();
  }

  public void Resume()
  {
    _pauseService.Reset();
  }

  public void ClearLastError()
  {
    _lastError = null;
  }

  public void SetLastError(string error)
  {
    _lastError = error;
  }

  public async ValueTask DisposeAsync()
  {
    _application?.Dispose();

    if (_runtime is not null)
    {
      await _runtime.DisposeAsync();
    }

    _initializationSync.Dispose();
  }
}
