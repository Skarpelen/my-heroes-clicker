using Microsoft.Playwright;
using MyHeroesClicker.Core.Models.Scenario;

namespace MyHeroesClicker.Browser.Browser;

public sealed class ScenarioBrowserTabManager
{
  private readonly BrowserSession _browserSession;
  private readonly Dictionary<ScenarioBrowserTabKind, ScenarioBrowserTab> _tabs = new();
  private readonly SemaphoreSlim _sync = new(1, 1);

  public ScenarioBrowserTabManager(BrowserSession browserSession)
  {
    _browserSession = browserSession;
  }

  public async Task<ScenarioBrowserTab> GetOrCreateAsync(
    ScenarioBrowserTabKind kind,
    string name,
    CancellationToken cancellationToken)
  {
    await _sync.WaitAsync(cancellationToken);

    try
    {
      if (_tabs.TryGetValue(kind, out var tab) && !tab.Page.IsClosed)
      {
        await tab.Page.BringToFrontAsync();

        return tab;
      }

      var page = kind == ScenarioBrowserTabKind.Main
        ? await GetMainPageAsync()
        : await _browserSession.Context.NewPageAsync();

      tab = new ScenarioBrowserTab(kind, name, page);
      _tabs[kind] = tab;

      await page.BringToFrontAsync();

      return tab;
    }
    finally
    {
      _sync.Release();
    }
  }

  private async Task<IPage> GetMainPageAsync()
  {
    if (!_browserSession.Page.IsClosed)
    {
      return _browserSession.Page;
    }

    return await _browserSession.Context.NewPageAsync();
  }
}
