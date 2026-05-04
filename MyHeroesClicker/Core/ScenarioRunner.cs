using Microsoft.Playwright;

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
    var retryCount = 0;

    while (currentStep != ScenarioStepKind.Stop)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (context.PauseService.IsPauseRequested)
      {
        return;
      }

      currentStep = await ResolveCurrentStepAsync(context, currentStep, cancellationToken);

      if (!_steps.TryGetValue(currentStep, out var step))
      {
        throw new InvalidOperationException($"Шаг {currentStep} не зарегистрирован.");
      }

      context.Logger.Log($"Выполняется шаг: {currentStep}");

      try
      {
        var result = await step.ExecuteAsync(context, cancellationToken);

        retryCount = 0;
        currentStep = result.NextStep;
      }
      catch (TimeoutException exception) when (retryCount < context.Options.MaxStepRetryCount)
      {
        retryCount++;

        context.Logger.Log($"Таймаут шага {currentStep}. Попытка {retryCount}/{context.Options.MaxStepRetryCount}: {exception.Message}");

        await Task.Delay(context.Options.RetryDelayMs, cancellationToken);

        currentStep = await ResolveCurrentStepAsync(context, currentStep, cancellationToken);
      }
      catch (PlaywrightException exception) when (IsTimeout(exception) && retryCount < context.Options.MaxStepRetryCount)
      {
        retryCount++;

        context.Logger.Log($"Таймаут Playwright на шаге {currentStep}. Попытка {retryCount}/{context.Options.MaxStepRetryCount}: {exception.Message}");

        await Task.Delay(context.Options.RetryDelayMs, cancellationToken);

        currentStep = await ResolveCurrentStepAsync(context, currentStep, cancellationToken);
      }
    }
  }

  private async Task<ScenarioStepKind> ResolveCurrentStepAsync(
    ScenarioContext context,
    ScenarioStepKind expectedStep,
    CancellationToken cancellationToken)
  {
    if (_steps.TryGetValue(expectedStep, out var expected) && await expected.CanHandleAsync(context, cancellationToken))
    {
      return expectedStep;
    }

    foreach (var step in _steps.Values)
    {
      if (step.Kind == ScenarioStepKind.Preparation || step.Kind == expectedStep)
      {
        continue;
      }

      if (await step.CanHandleAsync(context, cancellationToken))
      {
        context.Logger.Log($"Ожидался шаг {expectedStep}, но текущая страница соответствует шагу {step.Kind}. Продолжаю с него.");

        return step.Kind;
      }
    }

    return expectedStep;
  }

  private static bool IsTimeout(PlaywrightException exception)
  {
    return exception.Message.Contains("Timeout", StringComparison.OrdinalIgnoreCase);
  }
}
