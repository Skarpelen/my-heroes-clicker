using Microsoft.Playwright;
using MyHeroesClicker.Core.Models.Scenario;

namespace MyHeroesClicker.Browser.Browser;

public static class BattlePageLocators
{
  public static async Task<ILocator> AttackButtonAsync(IPage page, FarmLocation location)
  {
    if (location != FarmLocation.Adventure)
    {
      return page.Locator("a.btn_act[href='/batle1/attack10']");
    }

    var attack10Button = page.Locator("a.btn_act[href='/domp1/attack10']").First;

    if (await attack10Button.CountAsync() > 0)
    {
      return attack10Button;
    }

    return page.Locator("a.btn_act[href='/domp1/attack']").First;
  }

  public static ILocator ExpiredActionError(IPage page)
  {
    return page.GetByText("Действие уже устарело", new()
    {
      Exact = false
    });
  }

  public static ILocator Health(IPage page)
  {
    return page.Locator("span.nobr:has(img[title='Здоровье'])").First;
  }

  public static ILocator LowZealWarning(IPage page)
  {
    return page
      .Locator("li:has(img[src='/img/icons/zeal.png'])")
      .Filter(new()
      {
        HasTextString = "Недостаточно рвения"
      })
      .First;
  }

  public static ILocator TooFastWarning(IPage page)
  {
    return page.GetByText("Слишком быстро. Повторите действие через", new()
    {
      Exact = false
    }).First;
  }

  public static ILocator ReturnToBattleButton(IPage page, FarmLocation location)
  {
    if (location == FarmLocation.Adventure)
    {
      return page.Locator("a.btn_use.fltl.btn_gspace[href='/domp1']").Filter(new()
      {
        HasTextString = "в бой"
      });
    }

    return page.Locator("a.btn_use.fltl.btn_gspace[href='/batle1']").Filter(new()
    {
      HasTextString = "поиск"
    });
  }
}
