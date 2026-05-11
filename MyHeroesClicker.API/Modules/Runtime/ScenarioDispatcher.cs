using MyHeroesClicker.Core.Models.Scenario;

namespace MyHeroesClicker.API.Modules.Runtime;

public sealed class ScenarioDispatcher : IDisposable
{
  private readonly Dictionary<ScenarioBrowserTabKind, SemaphoreSlim> _tabLocks = new();
  private readonly Dictionary<ScenarioConcurrencyGroup, SemaphoreSlim> _groupLocks = new();
  private readonly object _sync = new();

  public ScenarioDispatchLease TryAcquire(ScenarioCatalogEntry entry)
  {
    var tabSemaphore = GetSemaphore(entry.BrowserTabKind);

    if (!tabSemaphore.Wait(0))
    {
      throw new InvalidOperationException($"Вкладка \"{entry.BrowserTabName}\" уже занята другим сценарием.");
    }

    SemaphoreSlim? groupSemaphore = null;

    if (entry.ConcurrencyGroup is not null)
    {
      groupSemaphore = GetSemaphore(entry.ConcurrencyGroup.Value);

      if (!groupSemaphore.Wait(0))
      {
        tabSemaphore.Release();

        throw new InvalidOperationException(FormatBusyGroupMessage(entry.ConcurrencyGroup.Value));
      }
    }

    return new ScenarioDispatchLease(
      entry.BrowserTabKind,
      entry.BrowserTabName,
      tabSemaphore,
      groupSemaphore);
  }

  public void Dispose()
  {
    lock (_sync)
    {
      foreach (var semaphore in _tabLocks.Values)
      {
        semaphore.Dispose();
      }

      foreach (var semaphore in _groupLocks.Values)
      {
        semaphore.Dispose();
      }

      _tabLocks.Clear();
      _groupLocks.Clear();
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

  private SemaphoreSlim GetSemaphore(ScenarioConcurrencyGroup group)
  {
    lock (_sync)
    {
      if (!_groupLocks.TryGetValue(group, out var semaphore))
      {
        semaphore = new SemaphoreSlim(1, 1);
        _groupLocks.Add(group, semaphore);
      }

      return semaphore;
    }
  }

  private static string FormatBusyGroupMessage(ScenarioConcurrencyGroup group)
  {
    return group switch
    {
      ScenarioConcurrencyGroup.Farm => "Фарм уже выполняется. Сейчас нельзя одновременно запускать драку и приключения.",
      _ => "Группа сценариев уже занята другим запуском."
    };
  }
}
