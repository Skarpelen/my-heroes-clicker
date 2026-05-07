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
    var battleUrl = new Uri(new Uri(context.Options.BaseUrl), "/battle1").ToString();

    await page.GotoAsync(battleUrl, new()
    {
      WaitUntil = WaitUntilState.DOMContentLoaded
    });

    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    if (await context.Guard.IsLoginPageAsync(page))
    {
      throw new AuthenticationRequiredException("Для входа в бой требуется авторизация.");
    }

    await context.Guard.ExpectBattlePageAsync(context, cancellationToken);

    return new StepResult(ScenarioStepType.Attack);
  }
}
