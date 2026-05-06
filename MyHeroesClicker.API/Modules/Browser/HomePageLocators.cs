using Microsoft.Playwright;

namespace MyHeroesClicker.Browser;

public static class HomePageLocators
{
  public static ILocator EquipmentSlots(IPage page)
  {
    return page.Locator(".user_slots a[href^='/inventory/?slot=']");
  }

  public static ILocator EquipmentSlot(IPage page, int slot)
  {
    return page.Locator($".user_slots a[href='/inventory/?slot={slot}']");
  }
}
