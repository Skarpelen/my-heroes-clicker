using Microsoft.Playwright;
using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Models.Scenarios;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.API.Modules.Steps;

public sealed class EnterBattleStep : IScenarioStep
{
  private readonly FarmLocation _location;

  public EnterBattleStep()
    : this(FarmLocation.Battle)
  {
  }

  public EnterBattleStep(FarmLocation location)
  {
    _location = location;
  }

  public ScenarioStepType Type => ScenarioStepType.EnterBattle;

  public Task<bool> CanHandleAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    return Task.FromResult(true);
  }

  public async Task<StepResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    var page = context.Page;

    var primaryPath = _location == FarmLocation.Adventure
      ? "/domp1"
      : "/battle1";

    await GoToBattleAsync(context, primaryPath);

    if (await context.Guard.IsLoginPageAsync(page))
    {
      throw new AuthenticationRequiredException("Для входа в бой требуется авторизация.");
    }

    if (_location == FarmLocation.Battle && !await context.Guard.IsBattlePageAsync(page, _location))
    {
      context.Logger.Log($"Не удалось открыть /battle1, текущий URL: {page.Url}. Пробую /batle1.");
      await GoToBattleAsync(context, "/batle1");
    }

    await context.Guard.ExpectBattlePageAsync(context, _location, cancellationToken);

    return new StepResult(ScenarioStepType.Attack);
  }

  private static async Task GoToBattleAsync(ScenarioContext context, string path)
  {
    var battleUrl = new Uri(new Uri(context.Options.BaseUrl), path).ToString();

    await context.Page.GotoAsync(battleUrl, new()
    {
      WaitUntil = WaitUntilState.DOMContentLoaded
    });

    await context.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
  }
}
