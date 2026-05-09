using MyHeroesClicker.Core.Confs;
using MyHeroesClicker.Core.Contracts.Database;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.Browser.Browser;

public sealed class DirectEquipmentClient
{
  private readonly MyHeroesWebClient _webClient;
  private readonly EquipmentModeConfiguration _configuration;

  public DirectEquipmentClient(
    MyHeroesWebClient webClient,
    EquipmentModeConfiguration configuration)
  {
    _webClient = webClient;
    _configuration = configuration;
  }

  public Task ApplyFarmStyleAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    return ApplyAsync(context, _configuration.FarmSlots, "фарм", cancellationToken);
  }

  public Task ApplyCombatStyleAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    return ApplyAsync(context, _configuration.CombatSlots, "бой", cancellationToken);
  }

  private async Task ApplyAsync(
    ScenarioContext context,
    IReadOnlyCollection<EquipmentSlotConfiguration> slots,
    string styleName,
    CancellationToken cancellationToken)
  {
    var emptySlots = slots
      .Where(slot => slot.ShouldBeEmpty)
      .OrderBy(slot => slot.SlotNumber)
      .ToArray();

    var currentEmptyTargetSlots = emptySlots.Length == 0
      ? new Dictionary<int, CurrentEquipmentSlotResponse>()
      : (await new CurrentEquipmentReader(_webClient)
          .ReadAsync(emptySlots.Select(slot => slot.SlotNumber).ToArray(), cancellationToken))
        .ToDictionary(slot => slot.SlotNumber);

    foreach (var slot in emptySlots)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (!currentEmptyTargetSlots.TryGetValue(slot.SlotNumber, out var currentSlot) || currentSlot.ShouldBeEmpty)
      {
        context.Logger.Log($"Слот {slot.SlotNumber} для режима {styleName} уже пустой.");
        continue;
      }

      if (currentSlot.ItemId is null)
      {
        context.Logger.Warn($"Слот {slot.SlotNumber} для режима {styleName} должен быть пустым, но текущий ID вещи для снятия не найден.");
        continue;
      }

      await TryApplyItemCommandAsync(
        context,
        $"/inventory/undress/{currentSlot.ItemId.Value}",
        $"Снимаю вещь {currentSlot.ItemId.Value} из слота {slot.SlotNumber} для режима {styleName} прямым запросом.",
        $"Не удалось снять вещь {currentSlot.ItemId.Value} из слота {slot.SlotNumber}. Считаю это допустимым, если вещь уже снята.",
        cancellationToken);
    }

    foreach (var slot in slots.Where(slot => !slot.ShouldBeEmpty).OrderBy(slot => slot.SlotNumber))
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (slot.ItemId is null)
      {
        context.Logger.Warn($"В слоте {slot.SlotNumber} для режима {styleName} не задан ID вещи. Пропускаю слот.");
        continue;
      }

      await TryApplyItemCommandAsync(
        context,
        $"/inventory/dress/{slot.ItemId.Value}",
        $"Надеваю вещь {slot.ItemId.Value} в слот {slot.SlotNumber} для режима {styleName} прямым запросом.",
        $"Не удалось надеть вещь {slot.ItemId.Value} в слот {slot.SlotNumber}. Считаю это допустимым, если вещь уже надета.",
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
