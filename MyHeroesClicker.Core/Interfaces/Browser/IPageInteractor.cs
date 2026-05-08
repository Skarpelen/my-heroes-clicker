using Microsoft.Playwright;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.Core.Interfaces.Browser;

public interface IPageInteractor
{
  Task PrepareForClickAsync(
    ScenarioContext context,
    ILocator locator,
    CancellationToken cancellationToken);
}
