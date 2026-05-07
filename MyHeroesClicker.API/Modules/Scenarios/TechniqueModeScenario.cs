using MyHeroesClicker.Browser;
using MyHeroesClicker.Core;

namespace MyHeroesClicker.Scenarios;

public sealed class TechniqueModeScenario : IScenario
{
  private readonly DirectTechniqueClient _techniqueClient;
  private readonly TechniqueMode _mode;

  public TechniqueModeScenario(
    string name,
    DirectTechniqueClient techniqueClient,
    TechniqueMode mode)
  {
    Name = name;
    _techniqueClient = techniqueClient;
    _mode = mode;
  }

  public string Name { get; }

  public Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    if (_mode == TechniqueMode.Farm)
    {
      return _techniqueClient.ApplyFarmModeAsync(context, cancellationToken);
    }

    return _techniqueClient.ApplyCombatModeAsync(context, cancellationToken);
  }
}
public enum TechniqueMode
{
  Farm,
  Combat
}
