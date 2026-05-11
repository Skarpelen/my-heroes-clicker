using Microsoft.Playwright;
using MyHeroesClicker.API.Modules.Core;
using MyHeroesClicker.API.Modules.Scenarios;
using MyHeroesClicker.API.Modules.Services;
using MyHeroesClicker.Browser.Browser;
using MyHeroesClicker.Browser.Diagnostics;
using MyHeroesClicker.Core.Confs;
using MyHeroesClicker.Core.Models.Account;
using MyHeroesClicker.Core.Models.Configuration;
using MyHeroesClicker.Core.Interfaces.Browser;
using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Interfaces.Services;
using MyHeroesClicker.Core.Models.Scenario;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.API.Modules.Runtime;

public sealed class ClickerRuntime : IAsyncDisposable
{
  private readonly IPlaywright _playwright;
  private readonly BrowserSession _browserSession;
  private readonly ScenarioBrowserTabManager _tabManager;
  private readonly IBrowserGuard _guard;
  private readonly IPageInteractor _pageInteractor;
  private readonly IHumanDelayService _humanDelay;
  private readonly IRunLogger _logger;
  private readonly IPauseService _pauseService;
  private readonly ClickerOptions _options;

  private ClickerRuntime(
    IPlaywright playwright,
    BrowserSession browserSession,
    ScenarioBrowserTabManager tabManager,
    IBrowserGuard guard,
    IPageInteractor pageInteractor,
    IHumanDelayService humanDelay,
    IRunLogger logger,
    IPauseService pauseService,
    ClickerOptions options,
    ScenarioContext context,
    ScenarioCatalog scenarios,
    IAlertService alertService,
    CharacterStatsReader statsReader,
    CurrentEquipmentReader equipmentReader)
  {
    _playwright = playwright;
    _browserSession = browserSession;
    _tabManager = tabManager;
    _guard = guard;
    _pageInteractor = pageInteractor;
    _humanDelay = humanDelay;
    _logger = logger;
    _pauseService = pauseService;
    _options = options;
    Context = context;
    Scenarios = scenarios;
    AlertService = alertService;
    StatsReader = statsReader;
    EquipmentReader = equipmentReader;
  }

  public ScenarioContext Context { get; }

  public ScenarioCatalog Scenarios { get; }

  public IAlertService AlertService { get; }

  public CharacterStatsReader StatsReader { get; }

  public CurrentEquipmentReader EquipmentReader { get; }

  public async Task<ScenarioContext> CreateContextAsync(
    ScenarioCatalogEntry entry,
    ScenarioRunOptions runOptions,
    IScenarioCoordinator coordinator,
    CancellationToken cancellationToken)
  {
    var tab = await _tabManager.GetOrCreateAsync(
      entry.BrowserTabKind,
      entry.BrowserTabName,
      cancellationToken);
    var characterState = new CharacterState();
    var logger = new ScenarioTabRunLogger(_logger, tab.Name);

    return new ScenarioContext(
      tab.Page,
      _guard,
      _pageInteractor,
      _humanDelay,
      logger,
      AlertService,
      _pauseService,
      coordinator,
      _options,
      runOptions,
      characterState,
      tab.Kind,
      tab.Name);
  }

  public static async Task<ClickerRuntime> StartAsync(
    ClickerOptions options,
    AccountCredentialsResponse activeAccount,
    ScenarioConfiguration configuration,
    IRunLogger logger,
    IAlertService alertService,
    IPauseService pauseService,
    ScenarioRunOptions initialRunOptions)
  {
    var failureDumpService = new FailureDumpService();
    var guard = new BrowserGuard(alertService, failureDumpService);
    var pageInteractor = new PageInteractor();
    var humanDelay = new HumanDelayService(options);
    var resourcesReader = new BattleResourcesReader();

    var playwright = await Playwright.CreateAsync();
    var browserSession = await BrowserSession.StartAsync(playwright, options);
    var tabManager = new ScenarioBrowserTabManager(browserSession);
    var webClient = new MyHeroesWebClient(browserSession, options);
    var equipmentClient = new DirectEquipmentClient(webClient, configuration.Equipment);
    var techniqueClient = new DirectTechniqueClient(webClient, configuration.Techniques);
    var statsReader = new CharacterStatsReader(webClient);
    var equipmentReader = new CurrentEquipmentReader(webClient);

    var characterState = new CharacterState();
    var context = new ScenarioContext(
      browserSession.Page,
      guard,
      pageInteractor,
      humanDelay,
      logger,
      alertService,
      pauseService,
      EmptyScenarioCoordinator.Instance,
      options,
      initialRunOptions,
      characterState,
      ScenarioBrowserTabKind.Main,
      "Основная вкладка");

    IScenario authenticationScenario = new AuthenticationScenario(activeAccount, webClient);
    IScenario farmPreparationScenario = new FarmPreparationScenario(
      equipmentClient,
      techniqueClient,
      statsReader);
    IScenario combatPreparationScenario = new CombatPreparationScenario(
      equipmentClient,
      techniqueClient,
      statsReader);
    IScenario authenticatedFarmPreparationScenario = new AuthenticatedScenario(authenticationScenario, farmPreparationScenario);
    IScenario authenticatedCombatPreparationScenario = new AuthenticatedScenario(authenticationScenario, combatPreparationScenario);
    IScenario farmBattleScenario = new FarmBattleScenario(resourcesReader, authenticationScenario, FarmLocation.Battle);
    IScenario adventureFarmBattleScenario = new FarmBattleScenario(resourcesReader, authenticationScenario, FarmLocation.Adventure);
    IScenario warRegistrationScenario = new AuthenticatedScenario(
      authenticationScenario,
      new WarRegistrationScenario(webClient, combatPreparationScenario, configuration.War));
    IScenario farmCycleScenario = new AuthenticatedScenario(
      authenticationScenario,
      new CompositeScenario("Цикл фарма в драке", [
        farmPreparationScenario,
        farmBattleScenario
      ]));
    IScenario adventureFarmCycleScenario = new AuthenticatedScenario(
      authenticationScenario,
      new CompositeScenario("Цикл фарма в приключениях", [
        farmPreparationScenario,
        adventureFarmBattleScenario
      ]));

    var scenarios = new ScenarioCatalog(
      authenticationScenario,
      authenticatedFarmPreparationScenario,
      authenticatedCombatPreparationScenario,
      farmBattleScenario,
      farmCycleScenario,
      adventureFarmCycleScenario,
      warRegistrationScenario);

    return new ClickerRuntime(
      playwright,
      browserSession,
      tabManager,
      guard,
      pageInteractor,
      humanDelay,
      logger,
      pauseService,
      options,
      context,
      scenarios,
      alertService,
      statsReader,
      equipmentReader);
  }

  public async ValueTask DisposeAsync()
  {
    await _browserSession.DisposeAsync();
    _playwright.Dispose();
  }
}
