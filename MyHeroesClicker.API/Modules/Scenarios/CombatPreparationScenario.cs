using MyHeroesClicker.Confs;
using MyHeroesClicker.Core;
using MyHeroesClicker.Services;

namespace MyHeroesClicker.Scenarios;

public sealed class CombatPreparationScenario : IScenario
{
  private readonly IScenario _innerScenario;

  public CombatPreparationScenario(
    EquipmentStyleService equipmentStyleService,
    EquipmentStyleConfig combatStyleConfig,
    TechniquesConfig techniquesConfig)
  {
    _innerScenario = new CompositeScenario("Подготовка к бою", [
      new ApplyEquipmentStyleScenario("Переодевание в боевые вещи", equipmentStyleService, combatStyleConfig, "бой"),
      new TechniqueModeScenario(
        "Включение приемов",
        techniquesConfig.EnableAllForCombat ? "включить все боевые приемы" : "боевые приемы не настроены")
    ]);
  }

  public string Name => "Подготовка к бою";

  public Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    return _innerScenario.ExecuteAsync(context, cancellationToken);
  }
}
