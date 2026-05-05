namespace MyHeroesClicker.Services;

public interface IPauseService
{
  bool IsPauseRequested { get; }

  void Reset();
}
