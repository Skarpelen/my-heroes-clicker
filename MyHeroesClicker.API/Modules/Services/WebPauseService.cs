using MyHeroesClicker.Core.Interfaces.Services;

namespace MyHeroesClicker.API.Modules.Services;

public sealed class WebPauseService : IPauseService
{
  private readonly object _sync = new();
  private string? _pauseReason;
  private DateTimeOffset? _pauseRequestedAt;

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

  public DateTimeOffset? PauseRequestedAt
  {
    get
    {
      lock (_sync)
      {
        return _pauseRequestedAt;
      }
    }
  }

  public void Request(string reason)
  {
    lock (_sync)
    {
      if (_pauseReason is null)
      {
        _pauseRequestedAt = DateTimeOffset.UtcNow;
      }

      _pauseReason = reason;
    }
  }

  public void Reset()
  {
    lock (_sync)
    {
      _pauseReason = null;
      _pauseRequestedAt = null;
    }
  }
}
