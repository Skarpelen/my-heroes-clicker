using Microsoft.Playwright;
using MyHeroesClicker.Core.Models.Scenario;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.Core.Interfaces.Browser;

public interface IBrowserGuard
{
  Task<bool> IsBattlePageAsync(IPage page);

  Task<bool> IsBattlePageAsync(IPage page, FarmLocation location);

  Task<bool> IsBattleLogPageAsync(IPage page);

  Task<bool> IsBattleLogPageAsync(IPage page, FarmLocation location);

  Task<bool> IsLoginPageAsync(IPage page);

  Task<bool> IsCaptchaPageAsync(IPage page);

  Task EnsureNotCaptchaAsync(ScenarioContext context, CancellationToken cancellationToken);

  Task ExpectBattlePageAsync(ScenarioContext context, CancellationToken cancellationToken);

  Task ExpectBattlePageAsync(ScenarioContext context, FarmLocation location, CancellationToken cancellationToken);

  Task ExpectBattleLogPageAsync(ScenarioContext context, CancellationToken cancellationToken);

  Task ExpectBattleLogPageAsync(ScenarioContext context, FarmLocation location, CancellationToken cancellationToken);

  Task<bool> TryRecoverExpiredActionAsync(ScenarioContext context, CancellationToken cancellationToken);

  Task<bool> HasExpiredActionErrorAsync(IPage page);

  Task FailAsync(ScenarioContext context, string message, CancellationToken cancellationToken);
}
