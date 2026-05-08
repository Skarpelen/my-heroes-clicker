using Microsoft.Playwright;

namespace MyHeroesClicker.Core;

public interface IPageInteractor
{
  Task PrepareForClickAsync(
    ScenarioContext context,
    ILocator locator,
    CancellationToken cancellationToken);
}
