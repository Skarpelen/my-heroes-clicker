namespace MyHeroesClicker.Core;

public sealed class ScenarioRunner
{
  private readonly Dictionary<ScenarioStepKind, IScenarioStep> _steps;

  public ScenarioRunner(IEnumerable<IScenarioStep> steps)
  {
    _steps = steps.ToDictionary(step => step.Kind);
  }

  public async Task RunAsync(ScenarioContext context, CancellationToken cancellationToken, ScenarioStepKind initialStep = ScenarioStepKind.Preparation)
  {
    var currentStep = initialStep;

    while (currentStep != ScenarioStepKind.Stop)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (context.PauseService.IsPauseRequested)
      {
        return;
      }

      if (!_steps.TryGetValue(currentStep, out var step))
      {
        throw new InvalidOperationException($"Шаг {currentStep} не зарегистрирован.");
      }

      context.Logger.Log($"Выполняется шаг: {currentStep}");

      var result = await step.ExecuteAsync(context, cancellationToken);

      currentStep = result.NextStep;
    }
  }
}
