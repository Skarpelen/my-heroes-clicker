using MyHeroesClicker.Core.Models.Scenarios;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.Core.Interfaces.Scenarios;

public interface IScenarioStep
{
  ScenarioStepType Type { get; }

  Task<bool> CanHandleAsync(ScenarioContext context, CancellationToken cancellationToken);

  Task<StepResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken);
}
