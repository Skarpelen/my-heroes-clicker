using Microsoft.Playwright;

namespace MyHeroesClicker.Browser;

public static class InventoryPageLocators
{
  public static ILocator ItemBlocks(IPage page)
  {
    return page.Locator(".the_content_ins > .block_info_1, .the_content_ins > .block_info_2");
  }

  public static ILocator ItemImage(ILocator itemBlock)
  {
    return itemBlock.Locator(".shop_item img[src^='/img/items/']").First;
  }

  public static ILocator DressButton(ILocator itemBlock)
  {
    return itemBlock.Locator(".buttons_block a.btn_use[href^='/inventory/dress/']").First;
  }

  public static ILocator UndressButton(ILocator itemBlock)
  {
    return itemBlock.Locator(".buttons_block a.btn_use[href^='/inventory/undress/']").First;
  }

  public static ILocator NextPageButton(IPage page)
  {
    return page.Locator(".pagination a[rel='next']").First;
  }
}
