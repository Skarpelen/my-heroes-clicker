using MyHeroesClicker.Services;

namespace MyHeroesClicker.API.Services;

public sealed class WebAlertService : IAlertService
{
  public Task PlayAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}
