using Microsoft.Playwright;
using MyHeroesClicker.Browser;
using MyHeroesClicker.Confs;
using MyHeroesClicker.Core;

namespace MyHeroesClicker.Services;

public sealed class EquipmentStyleService
{
  private readonly CharacterEquipmentReader _equipmentReader;

  public EquipmentStyleService(CharacterEquipmentReader equipmentReader)
  {
    _equipmentReader = equipmentReader;
  }

  public async Task ApplyAsync(
    ScenarioContext context,
    EquipmentStyleConfig styleConfig,
    string styleName,
    CancellationToken cancellationToken)
  {
    if (styleConfig.Slots.Count == 0)
    {
      context.Logger.Log($"Стиль {styleName} не настроен в конфиге. Переодевание пропущено.");

      return;
    }

    await OpenHomePageAsync(context);

    var equippedItems = await _equipmentReader.ReadEquippedItemsAsync(context.Page);

    foreach (var (slot, slotConfig) in styleConfig.Slots.OrderBy(slot => slot.Key))
    {
      cancellationToken.ThrowIfCancellationRequested();

      var expectedImageSrc = CharacterEquipmentReader.NormalizeImageSrc(slotConfig.ExpectedImageSrc);

      if (equippedItems.TryGetValue(slot, out var currentImageSrc)
          && currentImageSrc == expectedImageSrc)
      {
        context.Logger.Log($"Слот {slot} уже соответствует стилю {styleName}: {expectedImageSrc}.");

        continue;
      }

      await OpenSlotAsync(context, slot, cancellationToken);

      if (slotConfig.ShouldBeEmpty)
      {
        await UndressCurrentItemAsync(context, slot, cancellationToken);
      }
      else
      {
        await DressExpectedItemAsync(context, slot, expectedImageSrc, cancellationToken);
      }

      await OpenHomePageAsync(context);
      equippedItems = await _equipmentReader.ReadEquippedItemsAsync(context.Page);
    }
  }

  private static async Task OpenHomePageAsync(ScenarioContext context)
  {
    await context.Page.GotoAsync(context.Options.BaseUrl, new()
    {
      WaitUntil = WaitUntilState.DOMContentLoaded
    });

    await context.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
  }

  private static async Task OpenSlotAsync(
    ScenarioContext context,
    int slot,
    CancellationToken cancellationToken)
  {
    var slotLink = HomePageLocators.EquipmentSlot(context.Page, slot);

    await context.PageInteractor.PrepareForClickAsync(context, slotLink, cancellationToken);

    await slotLink.ClickAsync();
    await context.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
  }

  private static async Task UndressCurrentItemAsync(
    ScenarioContext context,
    int slot,
    CancellationToken cancellationToken)
  {
    var firstItem = InventoryPageLocators.ItemBlocks(context.Page).First;
    var undressButton = InventoryPageLocators.UndressButton(firstItem);

    if (await undressButton.CountAsync() == 0)
    {
      context.Logger.Log($"Слот {slot} уже пуст.");

      return;
    }

    context.Logger.Log($"Снимаю вещь в слоте {slot}.");

    await context.PageInteractor.PrepareForClickAsync(context, undressButton, cancellationToken);
    await undressButton.ClickAsync();
    await context.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
  }

  private static async Task DressExpectedItemAsync(
    ScenarioContext context,
    int slot,
    string expectedImageSrc,
    CancellationToken cancellationToken)
  {
    while (true)
    {
      var itemBlocks = InventoryPageLocators.ItemBlocks(context.Page);
      var count = await itemBlocks.CountAsync();

      for (var index = 0; index < count; index++)
      {
        var itemBlock = itemBlocks.Nth(index);
        var imageSrc = await InventoryPageLocators.ItemImage(itemBlock).GetAttributeAsync("src");

        if (CharacterEquipmentReader.NormalizeImageSrc(imageSrc ?? string.Empty) != expectedImageSrc)
        {
          continue;
        }

        var dressButton = InventoryPageLocators.DressButton(itemBlock);

        if (await dressButton.CountAsync() == 0)
        {
          context.Logger.Log($"Вещь {expectedImageSrc} в слоте {slot} уже надета.");

          return;
        }

        context.Logger.Log($"Надеваю вещь {expectedImageSrc} в слот {slot}.");

        await context.PageInteractor.PrepareForClickAsync(context, dressButton, cancellationToken);
        await dressButton.ClickAsync();
        await context.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        return;
      }

      var nextPageButton = InventoryPageLocators.NextPageButton(context.Page);

      if (await nextPageButton.CountAsync() == 0)
      {
        throw new InvalidOperationException($"Не найдена вещь {expectedImageSrc} для слота {slot}.");
      }

      context.Logger.Log($"Вещь {expectedImageSrc} не найдена в слоте {slot} на текущей странице. Перехожу дальше.");

      await context.PageInteractor.PrepareForClickAsync(context, nextPageButton, cancellationToken);
      await nextPageButton.ClickAsync();
      await context.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
  }
}
