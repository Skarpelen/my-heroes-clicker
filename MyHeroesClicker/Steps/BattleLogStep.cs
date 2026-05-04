using Microsoft.Playwright;
using MyHeroesClicker.Browser;
using MyHeroesClicker.Core;

namespace MyHeroesClicker.Steps;

public sealed class BattleLogStep : IScenarioStep
{
  public ScenarioStepKind Kind => ScenarioStepKind.BattleLog;

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

      return new StepResult(ScenarioStepKind.Attack);
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
      return new StepResult(ScenarioStepKind.Stop);
    }

    return new StepResult(ScenarioStepKind.Attack);
  }
}
