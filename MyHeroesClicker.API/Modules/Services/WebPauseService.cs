using MyHeroesClicker.Services;

namespace MyHeroesClicker.API.Services;

public sealed class WebPauseService : IPauseService
{
  private readonly object _sync = new();
  private string? _pauseReason;

  public bool IsPauseRequested
  {
    get
    {
      lock (_sync)
      {
        return _pauseReason is not null;
      }
    }
  }

  public string? PauseReason
  {
    get
    {
      lock (_sync)
      {
        return _pauseReason;
      }
    }
  }

  public void Request(string reason)
  {
    lock (_sync)
    {
      _pauseReason = reason;
    }
  }

  public void Reset()
  {
    lock (_sync)
    {
      _pauseReason = null;
    }
  }
}
