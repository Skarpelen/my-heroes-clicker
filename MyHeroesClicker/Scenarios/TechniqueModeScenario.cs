using MyHeroesClicker.Core;

namespace MyHeroesClicker.Scenarios;

public sealed class TechniqueModeScenario : IScenario
{
  private readonly string _modeDescription;
  private readonly IReadOnlyCollection<string> _techniqueNames;

  public TechniqueModeScenario(
    string name,
    string modeDescription,
    IReadOnlyCollection<string>? techniqueNames = null)
  {
    Name = name;
    _modeDescription = modeDescription;
    _techniqueNames = techniqueNames ?? [];
  }

  public string Name { get; }

  public Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    if (_techniqueNames.Count == 0)
    {
      context.Logger.Log($"Сценарий приемов пока не реализован: {_modeDescription}.");

      return Task.CompletedTask;
    }

    var names = string.Join(", ", _techniqueNames);
    context.Logger.Log($"Сценарий приемов пока не реализован: {_modeDescription}. Приемы из конфига: {names}.");

    return Task.CompletedTask;
  }
}
