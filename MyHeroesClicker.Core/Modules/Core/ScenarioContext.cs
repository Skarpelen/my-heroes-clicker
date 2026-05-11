using Microsoft.Playwright;
using MyHeroesClicker.Core.Confs;
using MyHeroesClicker.Core.Interfaces.Browser;
using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Interfaces.Services;
using MyHeroesClicker.Core.Models.Scenario;

namespace MyHeroesClicker.Core.Modules.Core;

public sealed class ScenarioContext
{
  public ScenarioContext(
    IPage page,
    IBrowserGuard guard,
    IPageInteractor pageInteractor,
    IHumanDelayService humanDelay,
    IRunLogger logger,
    IAlertService alertService,
    IPauseService pauseService,
    IScenarioCoordinator coordinator,
    ClickerOptions options,
    ScenarioRunOptions runOptions,
    CharacterState characterState,
    ScenarioBrowserTabKind browserTabKind = ScenarioBrowserTabKind.Main,
    string browserTabName = "Основная вкладка")
  {
    Page = page;
    Guard = guard;
    PageInteractor = pageInteractor;
    HumanDelay = humanDelay;
    Logger = logger;
    AlertService = alertService;
    PauseService = pauseService;
    Coordinator = coordinator;
    Options = options;
    RunOptions = runOptions;
    CharacterState = characterState;
    BrowserTabKind = browserTabKind;
    BrowserTabName = browserTabName;
  }

  public IPage Page { get; }

  public IBrowserGuard Guard { get; }

  public IPageInteractor PageInteractor { get; }

  public IHumanDelayService HumanDelay { get; }

  public IRunLogger Logger { get; }

  public IAlertService AlertService { get; }

  public IPauseService PauseService { get; }

  public IScenarioCoordinator Coordinator { get; }

  public ClickerOptions Options { get; }

  public ScenarioRunOptions RunOptions { get; }

  public int CompletedIterations { get; set; }

  public CharacterState CharacterState { get; }

  public ScenarioBrowserTabKind BrowserTabKind { get; }

  public string BrowserTabName { get; }

  public string? StatusMessage { get; private set; }

  public DateTimeOffset? NextCheckAt { get; private set; }

  public void SetStatus(string statusMessage, DateTimeOffset? nextCheckAt = null)
  {
    StatusMessage = statusMessage;
    NextCheckAt = nextCheckAt;
  }
}
