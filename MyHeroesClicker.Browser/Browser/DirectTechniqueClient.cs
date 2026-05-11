using MyHeroesClicker.Core.Models.Configuration;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.Browser.Browser;

public sealed class DirectTechniqueClient
{
  private static readonly HashSet<int> TechniqueIds = Enumerable.Range(1, 15).ToHashSet();

  private readonly MyHeroesWebClient _webClient;
  private readonly TechniqueModeConfiguration _configuration;

  public DirectTechniqueClient(
    MyHeroesWebClient webClient,
    TechniqueModeConfiguration configuration)
  {
    _webClient = webClient;
    _configuration = configuration;
  }

  public Task ApplyFarmModeAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    context.Logger.Log("Переключаю приемы в режим фарма прямыми запросами.");

    return ApplyAsync(context, _configuration.FarmEnabledTechniqueIds, cancellationToken);
  }

  public Task ApplyCombatModeAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    context.Logger.Log("Включаю боевые приемы прямыми запросами.");

    return ApplyAsync(context, _configuration.CombatEnabledTechniqueIds, cancellationToken);
  }

  private async Task ApplyAsync(
    ScenarioContext context,
    IReadOnlySet<int> enabledIds,
    CancellationToken cancellationToken)
  {
    foreach (var techniqueId in TechniqueIds)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var action = enabledIds.Contains(techniqueId)
        ? "activate"
        : "deactivate";

      var response = await _webClient.TryGetExpectedAsync($"/techniques/{action}/{techniqueId}", cancellationToken);

      if (!response.IsExpected)
      {
        context.Logger.Warn($"Не удалось выполнить {action} для приема {techniqueId}. Считаю это допустимым, если прием уже в нужном состоянии. Код ответа: {response.StatusCode}.");
      }
    }
  }
}
