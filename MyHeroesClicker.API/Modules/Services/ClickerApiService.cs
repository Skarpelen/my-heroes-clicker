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
      var scenarioConfiguration = await LoadScenarioConfigurationAsync(activeAccount.Id, cancellationToken);
      _runtime = await ClickerRuntime.StartAsync(
        _options,
        activeAccount,
        scenarioConfiguration,
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

  public async Task<IReadOnlyCollection<EquipmentSetSlotResponse>> ReadCurrentEquipmentSetAsync(
    long equipmentSetId,
    IReadOnlyCollection<int> slotNumbers,
    CancellationToken cancellationToken)
  {
    var application = await GetApplicationAsync(1, cancellationToken);

    if (application.IsRunning)
    {
      throw new InvalidOperationException("Нельзя читать текущий сет, пока выполняется сценарий.");
    }

    if (_runtime is null)
    {
      throw new InvalidOperationException("Браузер не инициализирован.");
    }

    await application.Scenarios.Authentication.Scenario.ExecuteAsync(_runtime.Context, cancellationToken);

    var currentSlots = await _runtime.EquipmentReader.ReadAsync(slotNumbers, cancellationToken);

    using var scope = _scopeFactory.CreateScope();
    var equipmentSets = scope.ServiceProvider.GetRequiredService<IEquipmentSetRepository>();

    foreach (var slot in currentSlots)
    {
      await equipmentSets.UpsertSlotAsync(
        equipmentSetId,
        slot.SlotNumber,
        new UpsertEquipmentSetSlotRequest(slot.ItemId, slot.ExpectedImageSrc, slot.ShouldBeEmpty),
        cancellationToken);
    }

    return await equipmentSets.GetSlotsAsync(equipmentSetId, cancellationToken);
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

  private async Task<ScenarioConfiguration> LoadScenarioConfigurationAsync(
    long accountId,
    CancellationToken cancellationToken)
  {
    using var scope = _scopeFactory.CreateScope();
    var equipmentSets = scope.ServiceProvider.GetRequiredService<IEquipmentSetRepository>();
    var techniquePresets = scope.ServiceProvider.GetRequiredService<ITechniquePresetRepository>();

    return new ScenarioConfiguration(
      new EquipmentModeConfiguration(
        await LoadEquipmentSlotsAsync(equipmentSets, accountId, "farm", cancellationToken),
        await LoadEquipmentSlotsAsync(equipmentSets, accountId, "combat", cancellationToken)),
      new TechniqueModeConfiguration(
        await LoadTechniqueIdsAsync(techniquePresets, accountId, "farm", cancellationToken),
        await LoadTechniqueIdsAsync(techniquePresets, accountId, "combat", cancellationToken)));
  }

  private static async Task<IReadOnlyCollection<EquipmentSlotConfiguration>> LoadEquipmentSlotsAsync(
    IEquipmentSetRepository equipmentSets,
    long accountId,
    string kind,
    CancellationToken cancellationToken)
  {
    var set = await FindConfigurationAsync(
      accountId,
      kind,
      equipmentSets.GetAllAsync,
      configuration => configuration.AccountId,
      cancellationToken);

    if (set is null)
    {
      throw new InvalidOperationException($"Не найден сет экипировки для режима {kind}. Создайте сет в настройках.");
    }

    var slots = await equipmentSets.GetSlotsAsync(set.Id, cancellationToken);

    if (slots.Count == 0)
    {
      throw new InvalidOperationException($"В сете экипировки для режима {kind} нет слотов. Сохраните текущую экипировку в настройках.");
    }

    return slots
      .Select(slot => new EquipmentSlotConfiguration(
        slot.SlotNumber,
        slot.ItemId,
        slot.ShouldBeEmpty))
      .ToArray();
  }

  private static async Task<IReadOnlySet<int>> LoadTechniqueIdsAsync(
    ITechniquePresetRepository techniquePresets,
    long accountId,
    string kind,
    CancellationToken cancellationToken)
  {
    var preset = await FindConfigurationAsync(
      accountId,
      kind,
      techniquePresets.GetAllAsync,
      configuration => configuration.AccountId,
      cancellationToken);

    if (preset is null)
    {
      throw new InvalidOperationException($"Не найден пресет приемов для режима {kind}. Создайте пресет в настройках.");
    }

    var slots = await techniquePresets.GetSlotsAsync(preset.Id, cancellationToken);

    if (slots.Count == 0)
    {
      throw new InvalidOperationException($"В пресете приемов для режима {kind} нет приемов. Сохраните приемы в настройках.");
    }

    return slots
      .Where(slot => slot.IsEnabled)
      .Select(slot => slot.TechniqueNumber)
      .ToHashSet();
  }

  private static async Task<TConfiguration?> FindConfigurationAsync<TConfiguration>(
    long accountId,
    string kind,
    Func<long?, string?, CancellationToken, Task<IReadOnlyCollection<TConfiguration>>> loadAsync,
    Func<TConfiguration, long?> getAccountId,
    CancellationToken cancellationToken)
  {
    var accountConfigurations = await loadAsync(accountId, kind, cancellationToken);

    if (accountConfigurations.FirstOrDefault() is { } accountConfiguration)
    {
      return accountConfiguration;
    }

    var configurations = await loadAsync(null, kind, cancellationToken);

    return configurations
      .Where(configuration => getAccountId(configuration) is null)
      .FirstOrDefault();
  }
}
