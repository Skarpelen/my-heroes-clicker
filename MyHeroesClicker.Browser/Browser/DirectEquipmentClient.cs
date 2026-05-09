using MyHeroesClicker.Core.Confs;
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
    foreach (var slot in slots.Where(slot => slot.ShouldBeEmpty).OrderBy(slot => slot.SlotNumber))
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (slot.ItemId is null)
      {
        context.Logger.Warn($"Слот {slot.SlotNumber} для режима {styleName} должен быть пустым, но ID вещи для снятия не задан.");
        continue;
      }

      await TryApplyItemCommandAsync(
        context,
        $"/inventory/undress/{slot.ItemId.Value}",
        $"Снимаю вещь {slot.ItemId.Value} из слота {slot.SlotNumber} для режима {styleName} прямым запросом.",
        $"Не удалось снять вещь {slot.ItemId.Value} из слота {slot.SlotNumber}. Считаю это допустимым, если вещь уже снята.",
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
