namespace MyHeroesClicker.Services;

public interface IAlertService
{
  Task PlayAsync(CancellationToken cancellationToken);
}