namespace MyHeroesClicker.Core.Interfaces.Services;

public interface IHumanDelayService
{
  Task WaitBeforeActionAsync(CancellationToken cancellationToken);
}
