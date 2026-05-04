using System.Runtime.InteropServices;

namespace MyHeroesClicker.Services;

public sealed class ConsoleBeepAlertService : IAlertService
{
  private const uint MbIconHand = 0x00000010;
  private readonly IRunLogger _logger;

  public ConsoleBeepAlertService(IRunLogger logger)
  {
    _logger = logger;
  }

  public async Task PlayAsync(CancellationToken cancellationToken)
  {
    _logger.Log("ОШИБКА СЦЕНАРИЯ");

    for (var i = 0; i < 5; i++)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (OperatingSystem.IsWindows())
      {
        MessageBeep(MbIconHand);
      }

      Console.Write('\a');

      try
      {
        Console.Beep(1200, 700);
      }
      catch (PlatformNotSupportedException)
      {
      }

      await Task.Delay(250, cancellationToken);
    }
  }

  [DllImport("user32.dll")]
  private static extern bool MessageBeep(uint uType);
}
