using Microsoft.Playwright;
using MyHeroesClicker.Browser.Browser;
using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Models.Scenarios;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.API.Modules.Steps;

public sealed class BattleLogStep : IScenarioStep
{
  private readonly FarmLocation _location;

  public BattleLogStep()
    : this(FarmLocation.Battle)
  {
  }

  public BattleLogStep(FarmLocation location)
  {
    _location = location;
  }

  public ScenarioStepType Type => ScenarioStepType.BattleLog;

  public Task<bool> CanHandleAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    return context.Guard.IsBattleLogPageAsync(context.Page, _location);
  }

  public async Task<StepResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    var page = context.Page;

    if (await context.Guard.HasExpiredActionErrorAsync(page))
    {
      context.Logger.Warn("Действие устарело вместо открытия лога боя. Возвращаюсь к атаке.");

      await page.ReloadAsync(new()
      {
        WaitUntil = WaitUntilState.DOMContentLoaded
      });

      await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

      return new StepResult(ScenarioStepType.Attack);
    }

    await context.Guard.ExpectBattleLogPageAsync(context, _location, cancellationToken);

    var returnButton = BattlePageLocators.ReturnToBattleButton(page, _location);

    await context.PageInteractor.PrepareForClickAsync(context, returnButton, cancellationToken);

    await returnButton.ClickAsync();

    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    await context.Guard.ExpectBattlePageAsync(context, _location, cancellationToken);

    context.CompletedIterations++;

    if (context.RunOptions.IterationLimit is not null
        && context.CompletedIterations >= context.RunOptions.IterationLimit.Value)
    {
      return new StepResult(ScenarioStepType.Stop);
    }

    return new StepResult(ScenarioStepType.Attack);
  }
}
