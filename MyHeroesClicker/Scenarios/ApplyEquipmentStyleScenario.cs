using MyHeroesClicker.Confs;
using MyHeroesClicker.Core;
using MyHeroesClicker.Services;

namespace MyHeroesClicker.Scenarios;

public sealed class ApplyEquipmentStyleScenario : IScenario
{
  private readonly EquipmentStyleService _equipmentStyleService;
  private readonly EquipmentStyleConfig _styleConfig;
  private readonly string _styleName;

  public ApplyEquipmentStyleScenario(
    string name,
    EquipmentStyleService equipmentStyleService,
    EquipmentStyleConfig styleConfig,
    string styleName)
  {
    Name = name;
    _equipmentStyleService = equipmentStyleService;
    _styleConfig = styleConfig;
    _styleName = styleName;
  }

  public string Name { get; }

  public Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    return _equipmentStyleService.ApplyAsync(context, _styleConfig, _styleName, cancellationToken);
  }
}
