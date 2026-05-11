using MyHeroesClicker.Core.Models.Scenario;

namespace MyHeroesClicker.API.Modules.Runtime;

public sealed class ScenarioDispatchLease : IDisposable
{
  private readonly SemaphoreSlim _semaphore;
  private bool _isDisposed;

  public ScenarioDispatchLease(
    ScenarioBrowserTabKind tabKind,
    string tabName,
    SemaphoreSlim tabSemaphore,
    SemaphoreSlim? groupSemaphore)
  {
    TabKind = tabKind;
    TabName = tabName;
    _semaphore = tabSemaphore;
    GroupSemaphore = groupSemaphore;
  }

  public ScenarioBrowserTabKind TabKind { get; }

  public string TabName { get; }

  private SemaphoreSlim? GroupSemaphore { get; }

  public void Dispose()
  {
    if (_isDisposed)
    {
      return;
    }

    _isDisposed = true;
    _semaphore.Release();
    GroupSemaphore?.Release();
  }
}
