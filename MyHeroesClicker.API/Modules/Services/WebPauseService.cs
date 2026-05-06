using MyHeroesClicker.Services;

namespace MyHeroesClicker.API.Services;

public sealed class WebPauseService : IPauseService
{
  public bool IsPauseRequested => false;

  public void Reset()
  {
  }
}
