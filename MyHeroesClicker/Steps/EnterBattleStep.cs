using Microsoft.Playwright;
using MyHeroesClicker.Browser;
using MyHeroesClicker.Core;

namespace MyHeroesClicker.Steps;

public sealed class EnterBattleStep : IScenarioStep
{
  public ScenarioStepType Type => ScenarioStepType.EnterBattle;

  public Task<bool> CanHandleAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    return Task.FromResult(true);
  }

  public async Task<StepResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    var page = context.Page;

    await page.GotoAsync(context.Options.BaseUrl, new()
    {
      WaitUntil = WaitUntilState.DOMContentLoaded
    });

    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    if (await context.Guard.IsLoginPageAsync(page))
    {
      throw new AuthenticationRequiredException("Для входа в бой требуется авторизация.");
    }

    var battleLinks = BattlePageLocators.BattleLinks(page);
    var startButton = await battleLinks.CountAsync() > 0
      ? battleLinks.First
      : page.GetByText("начать игру").First;

    await context.PageInteractor.PrepareForClickAsync(context, startButton, cancellationToken);

    await Task.WhenAll(
      page.WaitForURLAsync("**/batle1"),
      startButton.ClickAsync());

    await context.Guard.ExpectBattlePageAsync(context, cancellationToken);

    return new StepResult(ScenarioStepType.Attack);
  }
}
