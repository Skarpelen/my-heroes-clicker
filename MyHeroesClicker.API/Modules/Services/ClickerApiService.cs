using MyHeroesClicker.API.Modules.Application;
using MyHeroesClicker.API.Modules.Runtime;
using MyHeroesClicker.Core.Confs;
using MyHeroesClicker.Core.Contracts;
using MyHeroesClicker.Core.Contracts.Database;
using MyHeroesClicker.Core.Interfaces.Repositories;
using MyHeroesClicker.Core.Interfaces.Services;

namespace MyHeroesClicker.API.Modules.Services;

public sealed class ClickerApiService : IAsyncDisposable
{
  private readonly SemaphoreSlim _initializationSync = new(1, 1);
  private readonly ClickerOptions _options;
  private readonly IServiceScopeFactory _scopeFactory;
  private readonly IRunLogger _logger;
  private readonly IAlertService _alertService;
  private readonly IPauseService _pauseService;
  private ClickerRuntime? _runtime;
  private ClickerApplication? _application;
  private string? _lastError;
  private string? _lastUserEvent;
  private DateTimeOffset? _lastUserEventAt;

  public ClickerApiService(
    ClickerOptions options,
    IServiceScopeFactory scopeFactory,
    IRunLogger logger,
    IAlertService alertService,
    IPauseService pauseService)
  {
    _options = options;
    _scopeFactory = scopeFactory;
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
        null,
        null,
        null,
        _pauseService.PauseReason,
        _pauseService.PauseRequestedAt,
        _lastUserEvent,
        _lastUserEventAt,
        null,
        null,
        null,
        _lastError);
    }

    return new ScenarioStatusResponse(
      true,
      _application.IsRunning,
      _pauseService.IsPauseRequested,
      _application.ActiveScenarioKey,
      _application.ActiveScenarioName,
      _application.BrowserTabName,
      _pauseService.PauseReason,
      _pauseService.PauseRequestedAt,
      _lastUserEvent,
      _lastUserEventAt,
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

      var activeAccount = await ApplyStoredSettingsAsync(cancellationToken);
      _runtime = await ClickerRuntime.StartAsync(
        _options,
        activeAccount,
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
    SetLastUserEvent("Остановка сценариев");
    _pauseService.Reset();
    _application?.StopCurrentScenario();
  }

  public void Resume()
  {
    SetLastUserEvent("Продолжение после паузы");
    _pauseService.Reset();
  }

  public void SetLastUserEvent(string eventName)
  {
    _lastUserEvent = eventName;
    _lastUserEventAt = DateTimeOffset.UtcNow;
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

  private async Task<AccountResponse> ApplyStoredSettingsAsync(CancellationToken cancellationToken)
  {
    using var scope = _scopeFactory.CreateScope();
    var settingsRepository = scope.ServiceProvider.GetRequiredService<IAppSettingsRepository>();
    var settings = await settingsRepository.GetAsync(cancellationToken);

    if (settings is null)
    {
      throw new InvalidOperationException("Настройки приложения не найдены. Запустите миграции БД.");
    }

    ApplySettings(settings);

    if (settings.ActiveAccountId is null)
    {
      throw new InvalidOperationException("Активный аккаунт не выбран. Выберите аккаунт в настройках.");
    }

    var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
    var account = await accountRepository.GetByIdAsync(settings.ActiveAccountId.Value, cancellationToken);

    if (account is null)
    {
      throw new InvalidOperationException("Активный аккаунт не найден. Выберите аккаунт в настройках.");
    }

    if (!account.IsEnabled)
    {
      throw new InvalidOperationException("Активный аккаунт отключен. Выберите включенный аккаунт в настройках.");
    }

    if (string.IsNullOrWhiteSpace(account.EncryptedPassword))
    {
      throw new InvalidOperationException("У активного аккаунта не заполнен пароль.");
    }

    return account;
  }

  private void ApplySettings(AppSettingsResponse settings)
  {
    _options.BaseUrl = settings.BaseUrl;
    _options.BrowserKind = settings.BrowserKind;
    _options.Headless = settings.Headless;

    if (!string.IsNullOrWhiteSpace(settings.UserDataDir))
    {
      _options.UserDataDir = settings.UserDataDir;
    }

    _options.MinDelayMs = settings.MinDelayMs;
    _options.MaxDelayMs = settings.MaxDelayMs;
    _options.DefaultTimeoutMs = settings.DefaultTimeoutMs;
    _options.HpRecoveryDelayMultiplier = settings.HpRecoveryDelayMultiplier;
    _options.MinAttackHealthPercent = settings.MinAttackHealthPercent;
    _options.MaxAttackHealthPercent = settings.MaxAttackHealthPercent;
    _options.MaxStepRetryCount = settings.MaxStepRetryCount;
    _options.RetryDelayMs = settings.RetryDelayMs;
    _options.AuthenticationRetryDelayMs = settings.AuthenticationRetryDelayMs;
  }
}
