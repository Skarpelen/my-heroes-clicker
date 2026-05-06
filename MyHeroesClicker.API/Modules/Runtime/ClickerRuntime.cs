using Microsoft.Playwright;
using MyHeroesClicker.Browser;
using MyHeroesClicker.Confs;
using MyHeroesClicker.Core;
using MyHeroesClicker.Diagnostics;
using MyHeroesClicker.Scenarios;
using MyHeroesClicker.Services;

namespace MyHeroesClicker.Runtime;

public sealed class ClickerRuntime : IAsyncDisposable
{
  private readonly IPlaywright _playwright;
  private readonly BrowserSession _browserSession;

  private ClickerRuntime(
    IPlaywright playwright,
    BrowserSession browserSession,
    ScenarioContext context,
    ScenarioCatalog scenarios,
    IAlertService alertService)
  {
    _playwright = playwright;
    _browserSession = browserSession;
    Context = context;
    Scenarios = scenarios;
    AlertService = alertService;
  }

  public ScenarioContext Context { get; }

  public ScenarioCatalog Scenarios { get; }

  public IAlertService AlertService { get; }

  public static async Task<ClickerRuntime> StartAsync(
    ClickerOptions options,
    ClickerConfig config,
    IRunLogger logger,
    IAlertService alertService,
    IPauseService pauseService,
    int targetIterations,
    int maxHealth)
  {
    var failureDumpService = new FailureDumpService();
    var guard = new BrowserGuard(alertService, failureDumpService);
    var pageInteractor = new PageInteractor();
    var humanDelay = new HumanDelayService(options);
    var resourcesReader = new BattleResourcesReader();
    var equipmentReader = new CharacterEquipmentReader();
    var equipmentStyleService = new EquipmentStyleService(equipmentReader);

    var playwright = await Playwright.CreateAsync();
    var browserSession = await BrowserSession.StartAsync(playwright, options);

    var characterState = new CharacterState(maxHealth);
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

    IScenario authenticationScenario = new AuthenticationScenario(config, browserSession.SaveAuthStateAsync);
    IScenario farmPreparationScenario = new FarmPreparationScenario(
      equipmentStyleService,
      config.Equipment.FarmStyle,
      config.Techniques);
    IScenario combatPreparationScenario = new CombatPreparationScenario(
      equipmentStyleService,
      config.Equipment.CombatStyle,
      config.Techniques);
    IScenario farmBattleScenario = new FarmBattleScenario(resourcesReader, authenticationScenario);
    IScenario farmCycleScenario = new AuthenticatedScenario(
      authenticationScenario,
      new CompositeScenario("Цикл фарма", [
        farmPreparationScenario,
        farmBattleScenario
      ]));

    var scenarios = new ScenarioCatalog(
      authenticationScenario,
      farmPreparationScenario,
      combatPreparationScenario,
      farmBattleScenario,
      farmCycleScenario);

    return new ClickerRuntime(playwright, browserSession, context, scenarios, alertService);
  }

  public async ValueTask DisposeAsync()
  {
    await _browserSession.DisposeAsync();
    _playwright.Dispose();
  }
}
