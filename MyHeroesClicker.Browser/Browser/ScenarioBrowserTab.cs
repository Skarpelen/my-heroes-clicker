using Microsoft.Playwright;
using MyHeroesClicker.Core.Models.Scenarios;

namespace MyHeroesClicker.Browser.Browser;

public sealed class ScenarioBrowserTab
{
  public ScenarioBrowserTab(
    ScenarioBrowserTabKind kind,
    string name,
    IPage page)
  {
    Kind = kind;
    Name = name;
    Page = page;
  }

  public ScenarioBrowserTabKind Kind { get; }

  public string Name { get; }

  public IPage Page { get; }
}
