using Microsoft.Playwright;
using MyHeroesClicker.Core.Confs;
using MyHeroesClicker.Core.Interfaces.Browser;
using MyHeroesClicker.Core.Interfaces.Services;
using MyHeroesClicker.Core.Models.Scenarios;

namespace MyHeroesClicker.Core.Modules.Core;

public sealed class ScenarioContext
{
  public ScenarioContext(
    IPage page,
    IBrowserGuard guard,
    IPageInteractor pageInteractor,
    IHumanDelayService humanDelay,
    IRunLogger logger,
    IPauseService pauseService,
    ClickerOptions options,
    int targetIterations,
    CharacterState characterState,
    ScenarioBrowserTabKind browserTabKind = ScenarioBrowserTabKind.Main,
    string browserTabName = "Основная вкладка")
  {
    Page = page;
    Guard = guard;
    PageInteractor = pageInteractor;
    HumanDelay = humanDelay;
    Logger = logger;
    PauseService = pauseService;
    Options = options;
    TargetIterations = targetIterations;
    CharacterState = characterState;
    BrowserTabKind = browserTabKind;
    BrowserTabName = browserTabName;
  }

  public IPage Page { get; }

  public IBrowserGuard Guard { get; }

  public IPageInteractor PageInteractor { get; }

  public IHumanDelayService HumanDelay { get; }

  public IRunLogger Logger { get; }

  public IPauseService PauseService { get; }

  public ClickerOptions Options { get; }

  public int TargetIterations { get; private set; }

  public int CompletedIterations { get; set; }

  public CharacterState CharacterState { get; }

  public ScenarioBrowserTabKind BrowserTabKind { get; }

  public string BrowserTabName { get; }

  public void ResetIterations(int targetIterations)
  {
    TargetIterations = targetIterations;
    CompletedIterations = 0;
  }
}
