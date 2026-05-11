using Microsoft.Playwright;
using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Models.Alert;
using MyHeroesClicker.Core.Models.Scenario;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.API.Modules.Core;

public sealed class ScenarioRunner
{
  private readonly Dictionary<ScenarioStepType, IScenarioStep> _steps;
  private readonly IScenario? _authenticationScenario;

  public ScenarioRunner(IEnumerable<IScenarioStep> steps, IScenario? authenticationScenario = null)
  {
    _steps = steps.ToDictionary(step => step.Type);
    _authenticationScenario = authenticationScenario;
  }

  public async Task RunAsync(ScenarioContext context, CancellationToken cancellationToken, ScenarioStepType initialStep)
  {
    var currentStep = initialStep;
    var retryCount = 0;

    while (currentStep != ScenarioStepType.Stop)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (context.PauseService.IsPauseRequested)
      {
        await WaitWhilePausedAsync(context, cancellationToken);
      }

      await context.Guard.EnsureNotCaptchaAsync(context, cancellationToken);

      if (await context.Guard.IsLoginPageAsync(context.Page))
      {
        currentStep = await RecoverAuthenticationAsync(context, cancellationToken, initialStep, false);

        continue;
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

        context.Logger.Warn($"Таймаут шага {currentStep}. Попытка {retryCount}/{context.Options.MaxStepRetryCount}: {exception.Message}");

        await Task.Delay(context.Options.RetryDelayMs, cancellationToken);

        currentStep = await ResolveCurrentStepAsync(context, currentStep, cancellationToken);
      }
      catch (PlaywrightException exception) when (IsTimeout(exception) && retryCount < context.Options.MaxStepRetryCount)
      {
        retryCount++;

        context.Logger.Warn($"Таймаут Playwright на шаге {currentStep}. Попытка {retryCount}/{context.Options.MaxStepRetryCount}: {exception.Message}");

        await Task.Delay(context.Options.RetryDelayMs, cancellationToken);

        currentStep = await ResolveCurrentStepAsync(context, currentStep, cancellationToken);
      }
      catch (AuthenticationRequiredException)
      {
        retryCount = 0;
        currentStep = await RecoverAuthenticationAsync(context, cancellationToken, initialStep, true);
      }
    }
  }

  private async Task<ScenarioStepType> RecoverAuthenticationAsync(
    ScenarioContext context,
    CancellationToken cancellationToken,
    ScenarioStepType recoveryStep,
    bool waitBeforeAuthentication)
  {
    if (_authenticationScenario is null)
    {
      throw new InvalidOperationException("Требуется авторизация, но сценарий авторизации не зарегистрирован.");
    }

    if (waitBeforeAuthentication)
    {
      context.Logger.Warn($"Сессия сброшена. Повторная авторизация через {FormatDelay(context.Options.AuthenticationRetryDelayMs)}.");
      await Task.Delay(context.Options.AuthenticationRetryDelayMs, cancellationToken);
    }
    else
    {
      context.Logger.Log("Обнаружена страница авторизации.");
    }

    await context.AlertService.PublishAsync(
      AlertEventKind.AuthenticationRequired,
      "Обнаружена страница авторизации. Выполняю повторный вход.",
      cancellationToken);

    await _authenticationScenario.ExecuteAsync(context, cancellationToken);

    return recoveryStep;
  }

  private async Task<ScenarioStepType> ResolveCurrentStepAsync(
    ScenarioContext context,
    ScenarioStepType expectedStep,
    CancellationToken cancellationToken)
  {
    if (_steps.TryGetValue(expectedStep, out var expected) && await expected.CanHandleAsync(context, cancellationToken))
    {
      return expectedStep;
    }

    foreach (var step in _steps.Values)
    {
      if (step.Type == ScenarioStepType.EnterBattle || step.Type == expectedStep)
      {
        continue;
      }

      if (await step.CanHandleAsync(context, cancellationToken))
      {
        context.Logger.Warn($"Ожидался шаг {expectedStep}, но текущая страница соответствует шагу {step.Type}. Продолжаю с него.");

        return step.Type;
      }
    }

    return expectedStep;
  }

  private static bool IsTimeout(PlaywrightException exception)
  {
    return exception.Message.Contains("Timeout", StringComparison.OrdinalIgnoreCase);
  }

  private static string FormatDelay(int delayMs)
  {
    var delay = TimeSpan.FromMilliseconds(delayMs);

    if (delay.TotalMinutes >= 1)
    {
      return $"{(int)delay.TotalMinutes} мин {delay.Seconds} сек";
    }

    return $"{delay.Seconds} сек";
  }

  private static async Task WaitWhilePausedAsync(
    ScenarioContext context,
    CancellationToken cancellationToken)
  {
    context.Logger.Warn($"Сценарий на паузе: {context.PauseService.PauseReason ?? "причина не указана"}.");

    while (context.PauseService.IsPauseRequested)
    {
      cancellationToken.ThrowIfCancellationRequested();
      await Task.Delay(500, cancellationToken);
    }

    context.Logger.Log("Пауза снята. Продолжаю сценарий.");
  }
}
