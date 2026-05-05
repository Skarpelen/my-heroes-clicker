using Microsoft.Playwright;

namespace MyHeroesClicker.Browser;

public sealed class CharacterEquipmentReader
{
  public async Task<Dictionary<int, string>> ReadEquippedItemsAsync(IPage page)
  {
    var result = new Dictionary<int, string>();
    var slots = HomePageLocators.EquipmentSlots(page);
    var count = await slots.CountAsync();

    for (var index = 0; index < count; index++)
    {
      var slot = slots.Nth(index);
      var href = await slot.GetAttributeAsync("href");

      if (!TryReadSlotNumber(href, out var slotNumber))
      {
        continue;
      }

      var imageSrc = await slot.Locator("img").First.GetAttributeAsync("src");

      if (string.IsNullOrWhiteSpace(imageSrc))
      {
        continue;
      }

      result[slotNumber] = NormalizeImageSrc(imageSrc);
    }

    return result;
  }

  public static string NormalizeImageSrc(string imageSrc)
  {
    if (Uri.TryCreate(imageSrc, UriKind.Absolute, out var absoluteUri))
    {
      return absoluteUri.AbsolutePath;
    }

    return imageSrc;
  }

  private static bool TryReadSlotNumber(string? href, out int slot)
  {
    slot = 0;

    if (string.IsNullOrWhiteSpace(href))
    {
      return false;
    }

    var url = href.StartsWith("http", StringComparison.OrdinalIgnoreCase)
      ? href
      : "https://myheroes.ru" + href;

    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
    {
      return false;
    }

    var slotParameter = uri.Query
      .TrimStart('?')
      .Split('&', StringSplitOptions.RemoveEmptyEntries)
      .Select(part => part.Split('=', 2))
      .FirstOrDefault(parts => parts.Length == 2 && parts[0] == "slot");

    return slotParameter is not null && int.TryParse(slotParameter[1], out slot);
  }
}
