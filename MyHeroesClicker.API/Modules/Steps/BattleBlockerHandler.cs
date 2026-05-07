using Microsoft.Playwright;
using MyHeroesClicker.Browser;
using MyHeroesClicker.Core;

namespace MyHeroesClicker.Steps;

public sealed class BattleBlockerHandler
{
  private readonly BattleResourcesReader _resourcesReader;
  private readonly Random _random = new();

  public BattleBlockerHandler(BattleResourcesReader resourcesReader)
  {
    _resourcesReader = resourcesReader;
  }

  public async Task<StepResult?> TryHandleAsync(
    ScenarioContext context,
    CancellationToken cancellationToken)
  {
    if (await context.Guard.TryRecoverExpiredActionAsync(context, cancellationToken))
    {
      return new StepResult(ScenarioStepType.Attack);
    }

    if (await TryHandleTooFastWarningAsync(context, cancellationToken))
    {
      return new StepResult(ScenarioStepType.Attack);
    }

    if (await IsLowHealthAsync(context))
    {
      await WaitForHealthRecoveryAsync(context, cancellationToken);

      return new StepResult(ScenarioStepType.Attack);
    }

    if (await IsLowZealAsync(context))
    {
      await WaitForZealRecoveryAsync(context, cancellationToken);

      return new StepResult(ScenarioStepType.Attack);
    }

    return null;
  }

  private async Task<bool> TryHandleTooFastWarningAsync(
    ScenarioContext context,
    CancellationToken cancellationToken)
  {
    var warning = BattlePageLocators.TooFastWarning(context.Page);

    if (await warning.CountAsync() == 0)
    {
      return false;
    }

    context.Logger.Log("Слишком быстро. Повторяю атаку в обычном темпе.");

    await ReloadBattlePageAsync(context, cancellationToken);

    return true;
  }

  private async Task<bool> IsLowHealthAsync(ScenarioContext context)
  {
    var currentHealth = await _resourcesReader.ReadCurrentHealthAsync(context.Page);
    var minHealth = GetMinHealth(context);

    if (currentHealth >= minHealth)
    {
      return false;
    }

    context.Logger.Log($"Недостаточно здоровья для атаки: {currentHealth}/{context.CharacterState.MaxHealth}. Нужно минимум: {minHealth}.");

    return true;
  }

  private async Task WaitForHealthRecoveryAsync(
    ScenarioContext context,
    CancellationToken cancellationToken)
  {
    var page = context.Page;
    var minHealth = GetMinHealth(context);
    var previousHealth = await _resourcesReader.ReadCurrentHealthAsync(page);
    var previousCheckTime = DateTime.UtcNow;

    context.Logger.Log($"Ожидаю восстановления здоровья минимум до {minHealth} HP.");

    while (true)
    {
      cancellationToken.ThrowIfCancellationRequested();

      await WaitBeforeRecoveryCheckAsync(context, cancellationToken);
      await ReloadBattlePageAsync(context, cancellationToken);

      var currentHealth = await _resourcesReader.ReadCurrentHealthAsync(page);

      LogHealthRecoveryProgress(context, minHealth, previousHealth, previousCheckTime, currentHealth);

      previousHealth = currentHealth;
      previousCheckTime = DateTime.UtcNow;

      if (currentHealth >= minHealth)
      {
        return;
      }
    }
  }

  private async Task<bool> IsLowZealAsync(ScenarioContext context)
  {
    var warning = BattlePageLocators.LowZealWarning(context.Page);

    if (await warning.CountAsync() == 0)
    {
      return false;
    }

    var requiredZeal = await _resourcesReader.ReadRequiredZealAsync(warning);

    context.Logger.Log($"Недостаточно рвения для боя. Нужно минимум: {requiredZeal}.");

    return true;
  }

  private async Task WaitForZealRecoveryAsync(
    ScenarioContext context,
    CancellationToken cancellationToken)
  {
    var page = context.Page;
    var warning = BattlePageLocators.LowZealWarning(page);
    var requiredZeal = await _resourcesReader.ReadRequiredZealAsync(warning);

    context.Logger.Log($"Ожидаю восстановления рвения минимум до {requiredZeal}.");

    while (true)
    {
      cancellationToken.ThrowIfCancellationRequested();

      await WaitBeforeRecoveryCheckAsync(context, cancellationToken);
      await ReloadBattlePageAsync(context, cancellationToken);

      warning = BattlePageLocators.LowZealWarning(page);

      if (await warning.CountAsync() == 0)
      {
        context.Logger.Log("Рвения достаточно для боя.");

        return;
      }

      requiredZeal = await _resourcesReader.ReadRequiredZealAsync(warning);

      context.Logger.Log($"Рвения все еще недостаточно. Нужно минимум: {requiredZeal}.");
    }
  }

  private async Task WaitBeforeRecoveryCheckAsync(
    ScenarioContext context,
    CancellationToken cancellationToken)
  {
    var delay = _random.Next(
      context.Options.MinDelayMs * context.Options.HpRecoveryDelayMultiplier,
      context.Options.MaxDelayMs * context.Options.HpRecoveryDelayMultiplier + 1);

    await Task.Delay(delay, cancellationToken);
  }

  private static async Task ReloadBattlePageAsync(
    ScenarioContext context,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    await context.Page.ReloadAsync(new()
    {
      WaitUntil = WaitUntilState.DOMContentLoaded
    });

    await context.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    await context.Guard.ExpectBattlePageAsync(context, cancellationToken);
  }

  private static void LogHealthRecoveryProgress(
    ScenarioContext context,
    int minHealth,
    int previousHealth,
    DateTime previousCheckTime,
    int currentHealth)
  {
    var currentCheckTime = DateTime.UtcNow;
    var secondsPassed = (currentCheckTime - previousCheckTime).TotalSeconds;
    var healthRecovered = currentHealth - previousHealth;

    if (healthRecovered > 0 && secondsPassed > 0)
    {
      var recoveryPerSecond = healthRecovered / secondsPassed;
      var healthLeft = Math.Max(0, minHealth - currentHealth);
      var estimatedSecondsLeft = (int)Math.Ceiling(healthLeft / recoveryPerSecond);

      context.Logger.Log(
        $"Текущее здоровье: {currentHealth}/{context.CharacterState.MaxHealth}. Нужно минимум: {minHealth}. Примерно ждать: {FormatDuration(estimatedSecondsLeft)}.");

      return;
    }

    context.Logger.Log($"Текущее здоровье: {currentHealth}/{context.CharacterState.MaxHealth}. Нужно минимум: {minHealth}. Скорость восстановления пока неизвестна.");
  }

  private static string FormatDuration(int totalSeconds)
  {
    if (totalSeconds <= 0)
    {
      return "меньше секунды";
    }

    var duration = TimeSpan.FromSeconds(totalSeconds);

    if (duration.TotalHours >= 1)
    {
      return $"{(int)duration.TotalHours} ч {duration.Minutes} мин {duration.Seconds} сек";
    }

    if (duration.TotalMinutes >= 1)
    {
      return $"{duration.Minutes} мин {duration.Seconds} сек";
    }

    return $"{duration.Seconds} сек";
  }

  private int GetMinHealth(ScenarioContext context)
  {
    if (context.CharacterState.MaxHealth <= 0)
    {
      throw new InvalidOperationException("Максимальное здоровье не получено. Выполните подготовку режима перед фармом.");
    }

    var percent = context.Options.MinAttackHealthPercent
                  + _random.NextDouble() * (context.Options.MaxAttackHealthPercent - context.Options.MinAttackHealthPercent);

    return (int)Math.Ceiling(context.CharacterState.MaxHealth * percent);
  }
}
