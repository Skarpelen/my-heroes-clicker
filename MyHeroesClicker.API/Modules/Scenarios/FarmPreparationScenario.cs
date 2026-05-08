using MyHeroesClicker.API.Modules.Core;
using MyHeroesClicker.Browser.Browser;
using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.API.Modules.Scenarios;

public sealed class FarmPreparationScenario : IScenario
{
  private readonly IScenario _innerScenario;
  private readonly CharacterStatsReader _statsReader;

  public FarmPreparationScenario(
    DirectEquipmentClient equipmentClient,
    DirectTechniqueClient techniqueClient,
    CharacterStatsReader statsReader)
  {
    _statsReader = statsReader;
    _innerScenario = new CompositeScenario("Подготовка к фарму", [
      new ApplyEquipmentStyleScenario("Переодевание в фарм вещи", equipmentClient, EquipmentStyleMode.Farm),
      new TechniqueModeScenario(
        "Отключение лишних приемов",
        techniqueClient,
        TechniqueMode.Farm)
    ]);
  }

  public string Name => "Подготовка к фарму";

  public async Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    await _innerScenario.ExecuteAsync(context, cancellationToken);

    var maxHealth = await _statsReader.ReadMaxHealthAsync(cancellationToken);
    context.CharacterState.UpdateMaxHealth(maxHealth);
    context.Logger.Log($"Максимальное здоровье после подготовки к фарму: {maxHealth}.");
  }
}
