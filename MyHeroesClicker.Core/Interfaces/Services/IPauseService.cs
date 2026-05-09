namespace MyHeroesClicker.Core.Interfaces.Services;

public interface IPauseService
{
  bool IsPauseRequested { get; }

  string? PauseReason { get; }

  DateTimeOffset? PauseRequestedAt { get; }

  void Request(string reason);

  void Reset();
}
