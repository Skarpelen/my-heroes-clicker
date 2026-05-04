using Microsoft.Playwright;
using MyHeroesClicker.Browser;
using MyHeroesClicker.Core;

namespace MyHeroesClicker.Steps;

public sealed class AttackStep : IScenarioStep
{
  private readonly BattleBlockerHandler _blockerHandler;

  public AttackStep(BattleBlockerHandler blockerHandler)
  {
    _blockerHandler = blockerHandler;
  }

  public ScenarioStepKind Kind => ScenarioStepKind.Attack;

  public Task<bool> CanHandleAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    return context.Guard.IsBattlePageAsync(context.Page);
  }

  public async Task<StepResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    var page = context.Page;

    await context.Guard.ExpectBattlePageAsync(context, cancellationToken);

    var blockerResult = await _blockerHandler.TryHandleAsync(context, cancellationToken);

    if (blockerResult is not null)
    {
      return blockerResult;
    }

    var attackButton = BattlePageLocators.AttackButton(page);

    await context.PageInteractor.PrepareForClickAsync(context, attackButton, cancellationToken);

    await attackButton.ClickAsync();

    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    if (IsBattleLogPage(page))
    {
      return new StepResult(ScenarioStepKind.BattleLog);
    }

    blockerResult = await _blockerHandler.TryHandleAsync(context, cancellationToken);

    if (blockerResult is not null)
    {
      return blockerResult;
    }

    await context.Guard.FailAsync(
      context,
      $"После атаки ожидалась страница логов или исправляемая ошибка боя, но текущий URL: {page.Url}",
      cancellationToken);

    throw new InvalidOperationException($"Не удалось обработать результат атаки. Текущий URL: {page.Url}");
  }

  private static bool IsBattleLogPage(IPage page)
  {
    return Uri.TryCreate(page.Url, UriKind.Absolute, out var uri)
           && uri.Scheme == "https"
           && uri.Host == "myheroes.ru"
           && uri.AbsolutePath.StartsWith("/batle1/log/", StringComparison.OrdinalIgnoreCase);
  }
}
