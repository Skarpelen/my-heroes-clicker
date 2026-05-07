namespace MyHeroesClicker.Services;

public interface IHumanDelayService
{
  Task WaitBeforeActionAsync(CancellationToken cancellationToken);
}
