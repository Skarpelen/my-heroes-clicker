using Microsoft.Playwright;
using MyHeroesClicker.Browser;
using MyHeroesClicker.Confs;
using MyHeroesClicker.Core;
using MyHeroesClicker.Diagnostics;
using MyHeroesClicker.Scenarios;
using MyHeroesClicker.Services;

namespace MyHeroesClicker;

internal class Program
{
  private static async Task Main(string[] args)
  {
    var logger = new ConsoleRunLogger();

    var iterations = ReadPositiveInt("Сколько атак выполнить?", 500, logger);
    var maxHealth = ReadPositiveInt("Максимальное здоровье персонажа?", 3802, logger);

    var options = new ClickerOptions();
    var config = ClickerConfigLoader.Load(options, logger);

    using var cancellationTokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += (_, eventArgs) =>
    {
      eventArgs.Cancel = true;
      cancellationTokenSource.Cancel();
    };

    var pauseService = new PauseService();
    pauseService.StartListening(logger, cancellationTokenSource.Token);

    var alertService = new ConsoleBeepAlertService(logger);
    var failureDumpService = new FailureDumpService();
    var guard = new BrowserGuard(alertService, failureDumpService);
    var pageInteractor = new PageInteractor();
    var humanDelay = new HumanDelayService(options);
    var resourcesReader = new BattleResourcesReader();

    using var playwright = await Playwright.CreateAsync();
    await using var browserSession = await BrowserSession.StartAsync(playwright, options);

    var context = new ScenarioContext(
      browserSession.Page,
      guard,
      pageInteractor,
      humanDelay,
      logger,
      pauseService,
      options,
      iterations,
      maxHealth);

    IScenario authenticationScenario = new AuthenticationScenario(config, browserSession.SaveAuthStateAsync);
    IScenario preparationScenario = new FarmPreparationScenario();
    IScenario farmBattleScenario = new FarmBattleScenario(resourcesReader, authenticationScenario);
    var shouldRunPreparation = true;

    try
    {
      while (!cancellationTokenSource.Token.IsCancellationRequested)
      {
        logger.Log($"Выполняется сценарий: {authenticationScenario.Name}");
        await authenticationScenario.ExecuteAsync(context, cancellationTokenSource.Token);

        if (shouldRunPreparation)
        {
          logger.Log($"Выполняется сценарий: {preparationScenario.Name}");
          await preparationScenario.ExecuteAsync(context, cancellationTokenSource.Token);
          shouldRunPreparation = false;
        }

        logger.Log($"Выполняется сценарий: {farmBattleScenario.Name}");
        await farmBattleScenario.ExecuteAsync(context, cancellationTokenSource.Token);

        if (pauseService.IsPauseRequested)
        {
          logger.Log("Пауза активна. Можно вручную пользоваться сайтом.");
          logger.Log("Нажмите Enter в консоли, чтобы продолжить с главного меню.");

          Console.ReadLine();

          pauseService.Reset();

          await browserSession.Page.GotoAsync(options.BaseUrl, new()
          {
            WaitUntil = WaitUntilState.DOMContentLoaded
          });

          shouldRunPreparation = true;

          continue;
        }

        logger.Log($"Готово. Выполнено атак: {context.CompletedIterations}.");

        var nextIterations = ReadPositiveInt("Сколько дополнительных атак выполнить?", 500, logger);

        context.ResetIterations(nextIterations);
        shouldRunPreparation = true;
      }
    }
    catch (OperationCanceledException)
    {
      logger.Log("Остановлено пользователем.");
    }
    catch (Exception exception)
    {
      logger.Log(exception.Message);

      await alertService.PlayAsync(CancellationToken.None);

      logger.Log("Приложение остановлено на ошибке. Исправьте состояние в браузере и нажмите Enter для выхода.");
      Console.ReadLine();
    }
  }

  private static int ReadPositiveInt(string prompt, int defaultValue, IRunLogger logger)
  {
    while (true)
    {
      Console.Write($"{prompt} [{defaultValue}]: ");

      var input = Console.ReadLine();

      if (string.IsNullOrWhiteSpace(input))
      {
        return defaultValue;
      }

      if (int.TryParse(input, out var value) && value > 0)
      {
        return value;
      }

      logger.Log("Введите положительное целое число или оставьте пустым для значения по умолчанию.");
    }
  }
}
