using System;
using System.Collections.Generic;
using System.Text;

namespace MyHeroesClicker.Services;

public sealed class PauseService : IPauseService
{
  private volatile bool _pauseRequested;

  public bool IsPauseRequested => _pauseRequested;

  public void StartListening(IRunLogger logger, CancellationToken cancellationToken)
  {
    Task.Run(() =>
    {
      while (!cancellationToken.IsCancellationRequested)
      {
        var key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.P)
        {
          _pauseRequested = true;
          logger.Log("Запрошена пауза. Сценарий остановится после текущего шага.");
        }
      }
    }, cancellationToken);
  }

  public void Reset()
  {
    _pauseRequested = false;
  }
}
