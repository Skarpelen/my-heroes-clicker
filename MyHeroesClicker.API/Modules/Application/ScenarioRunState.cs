using MyHeroesClicker.API.Modules.Runtime;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.API.Modules.Application;

internal sealed class ScenarioRunState
{
  public ScenarioRunState(
    ScenarioCatalogEntry entry,
    ScenarioContext context,
    CancellationTokenSource cancellation)
  {
    RunId = Guid.NewGuid();
    Entry = entry;
    Context = context;
    Cancellation = cancellation;
    StartedAt = DateTimeOffset.UtcNow;
  }

  public Guid RunId { get; }

  public ScenarioCatalogEntry Entry { get; }

  public ScenarioContext Context { get; }

  public CancellationTokenSource Cancellation { get; }

  public DateTimeOffset StartedAt { get; }

  public DateTimeOffset? StopRequestedAt { get; private set; }

  public string? LastError { get; set; }

  public Task Task { get; set; } = Task.CompletedTask;

  public void RequestStop()
  {
    StopRequestedAt ??= DateTimeOffset.UtcNow;
    Cancellation.Cancel();
  }
}
