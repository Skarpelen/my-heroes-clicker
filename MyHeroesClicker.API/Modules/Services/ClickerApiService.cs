using MyHeroesClicker.API.Contracts;
using MyHeroesClicker.Application;
using MyHeroesClicker.Confs;
using MyHeroesClicker.Runtime;
using MyHeroesClicker.Services;

namespace MyHeroesClicker.API.Services;

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
      return new ScenarioStatusResponse(false, false, null, null, null, _lastError);
    }

    return new ScenarioStatusResponse(
      true,
      _application.IsRunning,
      _application.TargetIterations,
      _application.CompletedIterations,
      _application.MaxHealth,
      _application.LastError ?? _lastError);
  }

  public async Task<CharacterHealthResponse> GetCharacterHealthAsync(CancellationToken cancellationToken)
  {
    var application = await GetApplicationAsync(new ScenarioRunRequest(500), cancellationToken);
    var maxHealth = await application.RefreshMaxHealthAsync(cancellationToken);

    return new CharacterHealthResponse(maxHealth);
  }

  public async Task StartFarmAsync(ScenarioRunRequest request, CancellationToken cancellationToken)
  {
    ValidateRunRequest(request);

    var application = await GetApplicationAsync(request, cancellationToken);

    try
    {
      await application.StartFarmAsync(request.Iterations, cancellationToken);
      _lastError = null;
    }
    catch (Exception exception)
    {
      _lastError = exception.Message;
      throw;
    }
  }

  public async Task StartFarmPreparationAsync(CancellationToken cancellationToken)
  {
    var application = await GetApplicationAsync(new ScenarioRunRequest(500), cancellationToken);
    await StartScenarioAsync(application.StartFarmPreparationAsync, cancellationToken);
  }

  public async Task StartCombatPreparationAsync(CancellationToken cancellationToken)
  {
    var application = await GetApplicationAsync(new ScenarioRunRequest(500), cancellationToken);
    await StartScenarioAsync(application.StartCombatPreparationAsync, cancellationToken);
  }

  public void Stop()
  {
    _application?.StopCurrentScenario();
  }

  public async ValueTask DisposeAsync()
  {
    if (_runtime is not null)
    {
      await _runtime.DisposeAsync();
    }

    _initializationSync.Dispose();
  }

  private async Task<ClickerApplication> GetApplicationAsync(
    ScenarioRunRequest initialRequest,
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
        initialRequest.Iterations);

      _application = new ClickerApplication(_runtime);

      return _application;
    }
    finally
    {
      _initializationSync.Release();
    }
  }

  private async Task StartScenarioAsync(
    Func<CancellationToken, Task> startScenario,
    CancellationToken cancellationToken)
  {
    try
    {
      await startScenario(cancellationToken);
      _lastError = null;
    }
    catch (Exception exception)
    {
      _lastError = exception.Message;
      throw;
    }
  }

  private static void ValidateRunRequest(ScenarioRunRequest request)
  {
    if (request.Iterations <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(request.Iterations), "Iterations must be positive.");
    }
  }
}
