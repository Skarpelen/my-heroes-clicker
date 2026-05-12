using MyHeroesClicker.Core.Models.Alert;
using System.Runtime.Versioning;

namespace MyHeroesClicker.API.Modules.Services;

public sealed class AlertSoundService
{
  private readonly object _sync = new();
  private DateTimeOffset _lastSoundAt = DateTimeOffset.MinValue;

  public void Play(AlertEventKind kind)
  {
    if (kind is not (AlertEventKind.Captcha or AlertEventKind.FatalError))
    {
      return;
    }

    lock (_sync)
    {
      var now = DateTimeOffset.UtcNow;

      if (now - _lastSoundAt < TimeSpan.FromSeconds(2))
      {
        return;
      }

      _lastSoundAt = now;
    }

    _ = Task.Run(() => PlayCore(kind));
  }

  private static void PlayCore(AlertEventKind kind)
  {
    if (!OperatingSystem.IsWindows())
    {
      return;
    }

    PlayWindowsCore(kind);
  }

  [SupportedOSPlatform("windows")]
  private static void PlayWindowsCore(AlertEventKind kind)
  {
    try
    {
      if (kind == AlertEventKind.Captcha)
      {
        Console.Beep(1040, 180);
        Thread.Sleep(80);
        Console.Beep(1040, 180);
        Thread.Sleep(80);
        Console.Beep(1040, 260);

        return;
      }

      Console.Beep(320, 220);
      Thread.Sleep(40);
      Console.Beep(240, 320);
    }
    catch
    {
    }
  }
}
