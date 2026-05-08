using MyHeroesClicker.Core.Interfaces.Services;

namespace MyHeroesClicker.API.Modules.Services;

public sealed class WebAlertService : IAlertService
{
  public Task PlayAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}
