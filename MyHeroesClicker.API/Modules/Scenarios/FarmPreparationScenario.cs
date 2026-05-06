using MyHeroesClicker.Confs;
using MyHeroesClicker.Core;
using MyHeroesClicker.Services;

namespace MyHeroesClicker.Scenarios;

public sealed class FarmPreparationScenario : IScenario
{
  private readonly IScenario _innerScenario;

  public FarmPreparationScenario(
    EquipmentStyleService equipmentStyleService,
    EquipmentStyleConfig farmStyleConfig,
    TechniquesConfig techniquesConfig)
  {
    _innerScenario = new CompositeScenario("Подготовка к фарму", [
      new ApplyEquipmentStyleScenario("Переодевание в фарм вещи", equipmentStyleService, farmStyleConfig, "фарм"),
      new TechniqueModeScenario(
        "Отключение лишних приемов",
        "отключить приемы, не нужные для фарма",
        techniquesConfig.FarmDisabledTechniqueNames)
    ]);
  }

  public string Name => "Подготовка к фарму";

  public Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    return _innerScenario.ExecuteAsync(context, cancellationToken);
  }
}
