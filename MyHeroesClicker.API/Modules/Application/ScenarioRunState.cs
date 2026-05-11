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
    Entry = entry;
    Context = context;
    Cancellation = cancellation;
  }

  public ScenarioCatalogEntry Entry { get; }

  public ScenarioContext Context { get; }

  public CancellationTokenSource Cancellation { get; }

  public Task Task { get; set; } = Task.CompletedTask;
}
