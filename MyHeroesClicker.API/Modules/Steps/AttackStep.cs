using Microsoft.Playwright;
using MyHeroesClicker.Browser;
using MyHeroesClicker.Core;

namespace MyHeroesClicker.Steps;

public sealed class AttackStep : IScenarioStep
{
  private readonly BattleBlockerHandler _blockerHandler;
  private readonly FarmLocation _location;

  public AttackStep(BattleBlockerHandler blockerHandler)
    : this(blockerHandler, FarmLocation.Battle)
  {
  }

  public AttackStep(BattleBlockerHandler blockerHandler, FarmLocation location)
  {
    _blockerHandler = blockerHandler;
    _location = location;
  }

  public ScenarioStepType Type => ScenarioStepType.Attack;

  public Task<bool> CanHandleAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    return context.Guard.IsBattlePageAsync(context.Page, _location);
  }

  public async Task<StepResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    var page = context.Page;

    await context.Guard.ExpectBattlePageAsync(context, _location, cancellationToken);

    var blockerResult = await _blockerHandler.TryHandleAsync(context, cancellationToken);

    if (blockerResult is not null)
    {
      return blockerResult;
    }

    var attackButton = await BattlePageLocators.AttackButtonAsync(page, _location);

    await context.PageInteractor.PrepareForClickAsync(context, attackButton, cancellationToken);

    await attackButton.ClickAsync();

    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    if (IsBattleLogPage(page, _location))
    {
      return new StepResult(ScenarioStepType.BattleLog);
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

  private static bool IsBattleLogPage(IPage page, FarmLocation location)
  {
    if (location == FarmLocation.Adventure)
    {
      return Uri.TryCreate(page.Url, UriKind.Absolute, out var adventureUri)
             && adventureUri.Scheme == "https"
             && adventureUri.Host == "myheroes.ru"
             && adventureUri.AbsolutePath.StartsWith("/domp1/log/", StringComparison.OrdinalIgnoreCase);
    }

    return Uri.TryCreate(page.Url, UriKind.Absolute, out var uri)
           && uri.Scheme == "https"
           && uri.Host == "myheroes.ru"
           && (uri.AbsolutePath.StartsWith("/batle1/log/", StringComparison.OrdinalIgnoreCase)
               || uri.AbsolutePath.StartsWith("/battle1/log/", StringComparison.OrdinalIgnoreCase));
  }
}
