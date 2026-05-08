using Microsoft.Playwright;
using MyHeroesClicker.Browser;
using MyHeroesClicker.Browser.Browser;
using MyHeroesClicker.Core;
using MyHeroesClicker.Core.Confs;
using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Interfaces.Services;
using MyHeroesClicker.Core.Models.Scenarios;
using MyHeroesClicker.Core.Modules.Core;
using MyHeroesClicker.Diagnostics;
using MyHeroesClicker.Scenarios;
using MyHeroesClicker.Services;

namespace MyHeroesClicker.API.Modules.Runtime;

public sealed class ClickerRuntime : IAsyncDisposable
{
  private readonly IPlaywright _playwright;
  private readonly BrowserSession _browserSession;

  private ClickerRuntime(
    IPlaywright playwright,
    BrowserSession browserSession,
    ScenarioContext context,
    ScenarioCatalog scenarios,
    IAlertService alertService,
    CharacterStatsReader statsReader)
  {
    _playwright = playwright;
    _browserSession = browserSession;
    Context = context;
    Scenarios = scenarios;
    AlertService = alertService;
    StatsReader = statsReader;
  }

  public ScenarioContext Context { get; }

  public ScenarioCatalog Scenarios { get; }

  public IAlertService AlertService { get; }

  public CharacterStatsReader StatsReader { get; }

  public static async Task<ClickerRuntime> StartAsync(
    ClickerOptions options,
    ClickerConfig config,
    IRunLogger logger,
    IAlertService alertService,
    IPauseService pauseService,
    int targetIterations)
  {
    var failureDumpService = new FailureDumpService();
    var guard = new BrowserGuard(alertService, failureDumpService);
    var pageInteractor = new PageInteractor();
    var humanDelay = new HumanDelayService(options);
    var resourcesReader = new BattleResourcesReader();

    var playwright = await Playwright.CreateAsync();
    var browserSession = await BrowserSession.StartAsync(playwright, options);
    var webClient = new MyHeroesWebClient(browserSession, options);
    var equipmentClient = new DirectEquipmentClient(webClient);
    var techniqueClient = new DirectTechniqueClient(webClient);
    var statsReader = new CharacterStatsReader(webClient);

    var characterState = new CharacterState();
    var context = new ScenarioContext(
      browserSession.Page,
      guard,
      pageInteractor,
      humanDelay,
      logger,
      pauseService,
      options,
      targetIterations,
      characterState);

    IScenario authenticationScenario = new AuthenticationScenario(config, webClient, browserSession.SaveAuthStateAsync);
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
      adventureFarmCycleScenario);

    return new ClickerRuntime(playwright, browserSession, context, scenarios, alertService, statsReader);
  }

  public async ValueTask DisposeAsync()
  {
    await _browserSession.DisposeAsync();
    _playwright.Dispose();
  }
}
