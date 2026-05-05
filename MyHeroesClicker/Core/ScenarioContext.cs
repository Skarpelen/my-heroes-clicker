using Microsoft.Playwright;
using MyHeroesClicker.Browser;
using MyHeroesClicker.Services;

namespace MyHeroesClicker.Core;

public sealed class ScenarioContext
{
  public ScenarioContext(
    IPage page,
    BrowserGuard guard,
    PageInteractor pageInteractor,
    IHumanDelayService humanDelay,
    IRunLogger logger,
    IPauseService pauseService,
    ClickerOptions options,
    int targetIterations,
    CharacterState characterState)
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
  }

  public IPage Page { get; }

  public BrowserGuard Guard { get; }

  public PageInteractor PageInteractor { get; }

  public IHumanDelayService HumanDelay { get; }

  public IRunLogger Logger { get; }

  public IPauseService PauseService { get; }

  public ClickerOptions Options { get; }

  public int TargetIterations { get; private set; }

  public int CompletedIterations { get; set; }

  public CharacterState CharacterState { get; }

  public void ResetIterations(int targetIterations)
  {
    TargetIterations = targetIterations;
    CompletedIterations = 0;
  }
}
