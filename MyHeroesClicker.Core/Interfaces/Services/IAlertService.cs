namespace MyHeroesClicker.Core.Interfaces.Services;

public interface IAlertService
{
  Task PlayAsync(CancellationToken cancellationToken);
}
