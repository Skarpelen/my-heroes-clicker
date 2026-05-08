using MyHeroesClicker.Core.Models.Scenarios;

namespace MyHeroesClicker.API.Modules.Runtime;

public sealed class ScenarioDispatcher : IDisposable
{
  private readonly Dictionary<ScenarioBrowserTabKind, SemaphoreSlim> _tabLocks = new();
  private readonly object _sync = new();

  public ScenarioDispatchLease TryAcquire(ScenarioCatalogEntry entry)
  {
    var semaphore = GetSemaphore(entry.BrowserTabKind);

    if (!semaphore.Wait(0))
    {
      throw new InvalidOperationException($"Вкладка \"{entry.BrowserTabName}\" уже занята другим сценарием.");
    }

    return new ScenarioDispatchLease(entry.BrowserTabKind, entry.BrowserTabName, semaphore);
  }

  public void Dispose()
  {
    lock (_sync)
    {
      foreach (var semaphore in _tabLocks.Values)
      {
        semaphore.Dispose();
      }

      _tabLocks.Clear();
    }
  }

  private SemaphoreSlim GetSemaphore(ScenarioBrowserTabKind tabKind)
  {
    lock (_sync)
    {
      if (!_tabLocks.TryGetValue(tabKind, out var semaphore))
      {
        semaphore = new SemaphoreSlim(1, 1);
        _tabLocks.Add(tabKind, semaphore);
      }

      return semaphore;
    }
  }
}

public sealed class ScenarioDispatchLease : IDisposable
{
  private readonly SemaphoreSlim _semaphore;
  private bool _isDisposed;

  public ScenarioDispatchLease(
    ScenarioBrowserTabKind tabKind,
    string tabName,
    SemaphoreSlim semaphore)
  {
    TabKind = tabKind;
    TabName = tabName;
    _semaphore = semaphore;
  }

  public ScenarioBrowserTabKind TabKind { get; }

  public string TabName { get; }

  public void Dispose()
  {
    if (_isDisposed)
    {
      return;
    }

    _isDisposed = true;
    _semaphore.Release();
  }
}
