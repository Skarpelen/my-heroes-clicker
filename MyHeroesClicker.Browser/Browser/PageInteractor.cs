using Microsoft.Playwright;
using MyHeroesClicker.Core;

namespace MyHeroesClicker.Browser;

public sealed class PageInteractor : IPageInteractor
{
  public async Task PrepareForClickAsync(
    ScenarioContext context,
    ILocator locator,
    CancellationToken cancellationToken)
  {
    await locator.WaitForAsync(new()
    {
      State = WaitForSelectorState.Visible
    });

    await context.HumanDelay.WaitBeforeActionAsync(cancellationToken);

    await locator.ScrollIntoViewIfNeededAsync();
    await context.HumanDelay.WaitBeforeActionAsync(cancellationToken);

    await locator.HoverAsync();
    await context.HumanDelay.WaitBeforeActionAsync(cancellationToken);
  }
}
