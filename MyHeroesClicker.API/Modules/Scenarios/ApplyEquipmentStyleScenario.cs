using MyHeroesClicker.Browser.Browser;
using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.API.Modules.Scenarios;

public sealed class ApplyEquipmentStyleScenario : IScenario
{
  private readonly DirectEquipmentClient _equipmentClient;
  private readonly EquipmentStyleMode _mode;

  public ApplyEquipmentStyleScenario(
    string name,
    DirectEquipmentClient equipmentClient,
    EquipmentStyleMode mode)
  {
    Name = name;
    _equipmentClient = equipmentClient;
    _mode = mode;
  }

  public string Name { get; }

  public Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    if (_mode == EquipmentStyleMode.Farm)
    {
      return _equipmentClient.ApplyFarmStyleAsync(context, cancellationToken);
    }

    return _equipmentClient.ApplyCombatStyleAsync(context, cancellationToken);
  }
}

public enum EquipmentStyleMode
{
  Farm,
  Combat
}
