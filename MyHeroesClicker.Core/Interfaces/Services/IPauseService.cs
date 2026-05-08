namespace MyHeroesClicker.Services;

public interface IPauseService
{
  bool IsPauseRequested { get; }

  string? PauseReason { get; }

  void Request(string reason);

  void Reset();
}
