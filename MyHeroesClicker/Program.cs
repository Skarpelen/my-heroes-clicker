using Microsoft.Playwright;
using MyHeroesClicker.Confs;
using MyHeroesClicker.Runtime;
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

    await using var runtime = await ClickerRuntime.StartAsync(
      options,
      config,
      logger,
      alertService,
      pauseService,
      iterations,
      maxHealth);

    try
    {
      while (!cancellationTokenSource.Token.IsCancellationRequested)
      {
        logger.Log($"Выполняется сценарий: {runtime.Scenarios.FarmCycle.Name}");
        await runtime.Scenarios.FarmCycle.ExecuteAsync(runtime.Context, cancellationTokenSource.Token);

        if (pauseService.IsPauseRequested)
        {
          logger.Log("Пауза активна. Можно вручную пользоваться сайтом.");
          logger.Log("Нажмите Enter в консоли, чтобы продолжить с главного меню.");

          Console.ReadLine();

          pauseService.Reset();

          await runtime.Context.Page.GotoAsync(options.BaseUrl, new()
          {
            WaitUntil = WaitUntilState.DOMContentLoaded
          });

          continue;
        }

        logger.Log($"Готово. Выполнено атак: {runtime.Context.CompletedIterations}.");

        var nextIterations = ReadPositiveInt("Сколько дополнительных атак выполнить?", 500, logger);

        runtime.Context.ResetIterations(nextIterations);
      }
    }
    catch (OperationCanceledException)
    {
      logger.Log("Остановлено пользователем.");
    }
    catch (Exception exception)
    {
      logger.Log(exception.Message);

      await runtime.AlertService.PlayAsync(CancellationToken.None);

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
