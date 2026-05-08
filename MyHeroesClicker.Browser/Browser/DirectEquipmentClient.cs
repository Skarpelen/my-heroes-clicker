using MyHeroesClicker.Core;

namespace MyHeroesClicker.Browser;

public sealed class DirectEquipmentClient
{
  private static readonly IReadOnlyDictionary<int, int> FarmItemIds = new Dictionary<int, int>
  {
    [1] = 9165,
    [3] = 9166,
    [4] = 9167,
    [5] = 7881,
    [6] = 6035,
    [7] = 7533
  };

  private static readonly IReadOnlyDictionary<int, int> CombatItemIds = new Dictionary<int, int>
  {
    [1] = 9165,
    [2] = 9169,
    [3] = 9166,
    [4] = 9167,
    [5] = 7881,
    [6] = 7531,
    [7] = 7533,
    [8] = 7532
  };

  private static readonly IReadOnlyDictionary<int, int> FarmUndressItemIds = new Dictionary<int, int>
  {
    [2] = 9169,
    [8] = 7532
  };

  private static readonly IReadOnlyDictionary<int, int> EmptyUndressItemIds = new Dictionary<int, int>();

  private readonly MyHeroesWebClient _webClient;

  public DirectEquipmentClient(MyHeroesWebClient webClient)
  {
    _webClient = webClient;
  }

  public Task ApplyFarmStyleAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    return ApplyAsync(context, FarmItemIds, FarmUndressItemIds, "фарм", cancellationToken);
  }

  public Task ApplyCombatStyleAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    return ApplyAsync(context, CombatItemIds, EmptyUndressItemIds, "бой", cancellationToken);
  }

  private async Task ApplyAsync(
    ScenarioContext context,
    IReadOnlyDictionary<int, int> itemIdsBySlot,
    IReadOnlyDictionary<int, int> undressItemIdsBySlot,
    string styleName,
    CancellationToken cancellationToken)
  {
    foreach (var (slot, itemId) in undressItemIdsBySlot.OrderBy(item => item.Key))
    {
      cancellationToken.ThrowIfCancellationRequested();

      await TryApplyItemCommandAsync(
        context,
        $"/inventory/undress/{itemId}",
        $"Снимаю вещь {itemId} из слота {slot} для режима {styleName} прямым запросом.",
        $"Не удалось снять вещь {itemId} из слота {slot}. Считаю это допустимым, если вещь уже снята.",
        cancellationToken);
    }

    foreach (var (slot, itemId) in itemIdsBySlot.OrderBy(item => item.Key))
    {
      cancellationToken.ThrowIfCancellationRequested();

      await TryApplyItemCommandAsync(
        context,
        $"/inventory/dress/{itemId}",
        $"Надеваю вещь {itemId} в слот {slot} для режима {styleName} прямым запросом.",
        $"Не удалось надеть вещь {itemId} в слот {slot}. Считаю это допустимым, если вещь уже надета.",
        cancellationToken);
    }
  }

  private async Task TryApplyItemCommandAsync(
    ScenarioContext context,
    string path,
    string commandLogMessage,
    string toleratedErrorMessage,
    CancellationToken cancellationToken)
  {
    context.Logger.Log(commandLogMessage);

    var response = await _webClient.TryGetExpectedAsync(path, cancellationToken);

    if (!response.IsExpected)
    {
      context.Logger.Warn($"{toleratedErrorMessage} Код ответа: {response.StatusCode}.");
    }
  }
}
