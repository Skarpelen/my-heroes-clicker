using System.Text.RegularExpressions;
using MyHeroesClicker.Core.Models.EquipmentSet;

namespace MyHeroesClicker.Browser.Browser;

public sealed class CurrentEquipmentReader
{
  private static readonly Regex InventoryContentRegex = new(
    @"<div\s+class\s*=\s*[""'][^""']*\bthe_content_ins\b[^""']*[""'][^>]*>(?<content>.*)",
    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

  private static readonly Regex FirstItemRegex = new(
    @"<div\s+class\s*=\s*[""'][^""']*\bblock_info_\d+\b[^""']*[""'][^>]*>(?<item>.*?)(?=<div\s+class\s*=\s*[""'][^""']*\bblock_info_\d+\b|$)",
    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

  private static readonly Regex UndressLinkRegex = new(
    @"href\s*=\s*[""']/inventory/undress/(?<id>\d+)[""']",
    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

  private static readonly Regex ItemImageRegex = new(
    @"<img\s+[^>]*src\s*=\s*[""'](?<src>/img/items/[^""']+)[""']",
    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

  private readonly MyHeroesWebClient _webClient;

  public CurrentEquipmentReader(MyHeroesWebClient webClient)
  {
    _webClient = webClient;
  }

  public async Task<IReadOnlyCollection<CurrentEquipmentSlotResponse>> ReadAsync(
    IReadOnlyCollection<int> slotNumbers,
    CancellationToken cancellationToken)
  {
    var result = new List<CurrentEquipmentSlotResponse>();

    foreach (var slotNumber in slotNumbers.Order())
    {
      cancellationToken.ThrowIfCancellationRequested();

      result.Add(await ReadSlotAsync(slotNumber, cancellationToken));
    }

    return result;
  }

  private async Task<CurrentEquipmentSlotResponse> ReadSlotAsync(
    int slotNumber,
    CancellationToken cancellationToken)
  {
    var html = await _webClient.GetDocumentAsync($"/inventory/?slot={slotNumber}", cancellationToken);
    var contentMatch = InventoryContentRegex.Match(html);

    if (!contentMatch.Success)
    {
      throw new InvalidOperationException($"Не удалось найти блок инвентаря для слота {slotNumber}.");
    }

    var itemMatch = FirstItemRegex.Match(contentMatch.Groups["content"].Value);

    if (!itemMatch.Success)
    {
      return new CurrentEquipmentSlotResponse
      {
        SlotNumber = slotNumber,
        ExpectedImageSrc = string.Empty,
        ShouldBeEmpty = true
      };
    }

    var itemHtml = itemMatch.Groups["item"].Value;
    var imageSrc = ItemImageRegex.Match(itemHtml).Groups["src"].Value;
    var undressLink = UndressLinkRegex.Match(itemHtml);

    if (!undressLink.Success || !int.TryParse(undressLink.Groups["id"].Value, out var itemId))
    {
      return new CurrentEquipmentSlotResponse
      {
        SlotNumber = slotNumber,
        ExpectedImageSrc = imageSrc,
        ShouldBeEmpty = true
      };
    }

    return new CurrentEquipmentSlotResponse
    {
      SlotNumber = slotNumber,
      ItemId = itemId,
      ExpectedImageSrc = imageSrc,
      ShouldBeEmpty = false
    };
  }
}
