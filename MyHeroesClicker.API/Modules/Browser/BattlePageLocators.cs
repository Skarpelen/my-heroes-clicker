using Microsoft.Playwright;

namespace MyHeroesClicker.Browser;

public static class BattlePageLocators
{
  public static ILocator AttackButton(IPage page)
  {
    return page.Locator("a.btn_act[href='/batle1/attack10'], a.btn_act[href='/battle1/attack10']");
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

  public static ILocator ReturnToBattleButton(IPage page)
  {
    return page.Locator("a.btn_use.fltl.btn_gspace[href='/batle1'], a.btn_use.fltl.btn_gspace[href='/battle1']").Filter(new()
    {
      HasTextString = "поиск"
    });
  }
}
