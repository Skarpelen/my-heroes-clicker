using MyHeroesClicker.API.Modules.Core;
using MyHeroesClicker.Browser.Browser;
using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Models.Scenario;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.API.Modules.Scenarios;

public sealed class CombatPreparationScenario : IScenario
{
  private readonly IScenario _innerScenario;
  private readonly CharacterStatsReader _statsReader;

  public CombatPreparationScenario(
    DirectEquipmentClient equipmentClient,
    DirectTechniqueClient techniqueClient,
    CharacterStatsReader statsReader)
  {
    _statsReader = statsReader;
    _innerScenario = new CompositeScenario("Подготовка к бою", [
      new ApplyEquipmentStyleScenario("Переодевание в боевые вещи", equipmentClient, EquipmentStyleMode.Combat),
      new TechniqueModeScenario(
        "Включение приемов",
        techniqueClient,
        TechniqueMode.Combat)
    ]);
  }

  public string Name => "Подготовка к бою";

  public async Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    await _innerScenario.ExecuteAsync(context, cancellationToken);

    var maxHealth = await _statsReader.ReadMaxHealthAsync(cancellationToken);
    context.CharacterState.UpdateMaxHealth(maxHealth);
    context.Logger.Log($"Максимальное здоровье после подготовки к бою: {maxHealth}.");
  }
}
