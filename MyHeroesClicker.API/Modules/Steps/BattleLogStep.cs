using Microsoft.Playwright;
using MyHeroesClicker.Browser;
using MyHeroesClicker.Core;

namespace MyHeroesClicker.Steps;

public sealed class BattleLogStep : IScenarioStep
{
  public ScenarioStepType Type => ScenarioStepType.BattleLog;

  public Task<bool> CanHandleAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    return context.Guard.IsBattleLogPageAsync(context.Page);
  }

  public async Task<StepResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    var page = context.Page;

    if (await context.Guard.HasExpiredActionErrorAsync(page))
    {
      context.Logger.Log("Действие устарело вместо открытия лога боя. Возвращаюсь к атаке.");

      await page.ReloadAsync(new()
      {
        WaitUntil = WaitUntilState.DOMContentLoaded
      });

      await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

      return new StepResult(ScenarioStepType.Attack);
    }

    await context.Guard.ExpectBattleLogPageAsync(context, cancellationToken);

    var returnButton = BattlePageLocators.ReturnToBattleButton(page);

    await context.PageInteractor.PrepareForClickAsync(context, returnButton, cancellationToken);

    await returnButton.ClickAsync();

    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    await context.Guard.ExpectBattlePageAsync(context, cancellationToken);

    context.CompletedIterations++;

    if (context.CompletedIterations >= context.TargetIterations)
    {
      return new StepResult(ScenarioStepType.Stop);
    }

    return new StepResult(ScenarioStepType.Attack);
  }
}
