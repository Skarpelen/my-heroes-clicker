using Microsoft.Playwright;
using MyHeroesClicker.Browser.Diagnostics;
using MyHeroesClicker.Core.Interfaces.Browser;
using MyHeroesClicker.Core.Interfaces.Services;
using MyHeroesClicker.Core.Models.Scenarios;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.Browser.Browser;

public sealed class BrowserGuard : IBrowserGuard
{
  private readonly IAlertService _alertService;
  private readonly FailureDumpService _failureDumpService;

  public BrowserGuard(IAlertService alertService, FailureDumpService failureDumpService)
  {
    _alertService = alertService;
    _failureDumpService = failureDumpService;
  }

  public Task<bool> IsBattlePageAsync(IPage page)
  {
    return IsBattlePageAsync(page, FarmLocation.Battle);
  }

  public async Task<bool> IsBattlePageAsync(IPage page, FarmLocation location)
  {
    if (await IsLoginPageAsync(page))
    {
      return false;
    }

    if (!IsLocationPage(page, location))
    {
      return false;
    }

    return await (await BattlePageLocators.AttackButtonAsync(page, location)).CountAsync() > 0;
  }

  public Task<bool> IsBattleLogPageAsync(IPage page)
  {
    return IsBattleLogPageAsync(page, FarmLocation.Battle);
  }

  public async Task<bool> IsBattleLogPageAsync(IPage page, FarmLocation location)
  {
    if (await IsLoginPageAsync(page))
    {
      return false;
    }

    if (!Uri.TryCreate(page.Url, UriKind.Absolute, out var uri)
        || uri.Scheme != "https"
        || uri.Host != "myheroes.ru"
        || !IsBattleLogPath(uri.AbsolutePath, location))
    {
      return false;
    }

    return await BattlePageLocators.ReturnToBattleButton(page, location).CountAsync() > 0;
  }

  public async Task<bool> IsLoginPageAsync(IPage page)
  {
    if (!IsExpectedPage(page, "/"))
    {
      return false;
    }

    return await LoginPageLocators.LoginForm(page).CountAsync() > 0
           && await LoginPageLocators.LoginInput(page).CountAsync() > 0
           && await LoginPageLocators.PasswordInput(page).CountAsync() > 0
           && await LoginPageLocators.SubmitButton(page).CountAsync() > 0;
  }

  public Task<bool> IsCaptchaPageAsync(IPage page)
  {
    if (!Uri.TryCreate(page.Url, UriKind.Absolute, out var uri))
    {
      return Task.FromResult(false);
    }

    return Task.FromResult(
      uri.Scheme == "https"
      && uri.Host == "myheroes.ru"
      && uri.AbsolutePath.StartsWith("/capcha", StringComparison.OrdinalIgnoreCase));
  }

  public async Task EnsureNotCaptchaAsync(
    ScenarioContext context,
    CancellationToken cancellationToken)
  {
    const string reason = "Сайт открыл страницу capcha. Решите проверку вручную и нажмите продолжить.";

    while (await IsCaptchaPageAsync(context.Page))
    {
      if (!context.PauseService.IsPauseRequested)
      {
        context.Logger.Error(reason);
        context.PauseService.Request(reason);
        await _alertService.PlayAsync(cancellationToken);
      }

      while (context.PauseService.IsPauseRequested)
      {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Delay(500, cancellationToken);
      }
    }
  }

  public async Task ExpectBattlePageAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    await ExpectBattlePageAsync(context, FarmLocation.Battle, cancellationToken);
  }

  public async Task ExpectBattlePageAsync(
    ScenarioContext context,
    FarmLocation location,
    CancellationToken cancellationToken)
  {
    var page = context.Page;

    await EnsureNotCaptchaAsync(context, cancellationToken);

    if (!await IsBattlePageAsync(page, location))
    {
      await StopWithErrorAsync(context, $"Ожидалась страница боя, но текущий URL: {context.Page.Url}", cancellationToken);
    }

    var attackButton = await BattlePageLocators.AttackButtonAsync(page, location);

    if (await attackButton.CountAsync() == 0)
    {
      await StopWithErrorAsync(context, "Не найдена кнопка атаки.", cancellationToken);
    }
  }

  public async Task ExpectBattleLogPageAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    await ExpectBattleLogPageAsync(context, FarmLocation.Battle, cancellationToken);
  }

  public async Task ExpectBattleLogPageAsync(
    ScenarioContext context,
    FarmLocation location,
    CancellationToken cancellationToken)
  {
    var page = context.Page;

    await EnsureNotCaptchaAsync(context, cancellationToken);

    if (!await IsBattleLogPageAsync(page, location))
    {
      await StopWithErrorAsync(context, $"Ожидалась страница логов, но текущий URL: {context.Page.Url}", cancellationToken);
    }

    var returnButton = BattlePageLocators.ReturnToBattleButton(page, location);

    if (await returnButton.CountAsync() == 0)
    {
      await StopWithErrorAsync(context, "Не найдена кнопка возврата в бой.", cancellationToken);
    }
  }

  public async Task<bool> TryRecoverExpiredActionAsync(
    ScenarioContext context,
    CancellationToken cancellationToken)
  {
    var page = context.Page;
    var expiredActionError = BattlePageLocators.ExpiredActionError(page);

    if (await expiredActionError.CountAsync() == 0)
    {
      return false;
    }

    context.Logger.Warn("Действие устарело. Обновляю страницу и повторяю шаг.");

    await page.ReloadAsync(new()
    {
      WaitUntil = WaitUntilState.DOMContentLoaded
    });

    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    return true;
  }

  public async Task<bool> HasExpiredActionErrorAsync(IPage page)
  {
    return await BattlePageLocators.ExpiredActionError(page).CountAsync() > 0;
  }

  public Task FailAsync(
    ScenarioContext context,
    string message,
    CancellationToken cancellationToken)
  {
    return StopWithErrorAsync(context, message, cancellationToken);
  }

  private async Task StopWithErrorAsync(
      ScenarioContext context,
      string message,
      CancellationToken cancellationToken)
  {
    await EnsureNotCaptchaAsync(context, cancellationToken);

    if (await IsLoginPageAsync(context.Page))
    {
      throw new AuthenticationRequiredException("Сессия не авторизована.");
    }

    await _failureDumpService.SaveAsync(context.Page, message, cancellationToken);
    await _alertService.PlayAsync(cancellationToken);

    throw new InvalidOperationException(message);
  }

  private static bool IsExpectedPage(IPage page, string expectedPath)
  {
    if (!Uri.TryCreate(page.Url, UriKind.Absolute, out var uri))
    {
      return false;
    }

    return uri.Scheme == "https"
           && uri.Host == "myheroes.ru"
           && uri.AbsolutePath == expectedPath;
  }

  private static bool IsLocationPage(IPage page, FarmLocation location)
  {
    return location == FarmLocation.Adventure
      ? IsExpectedPage(page, "/domp1")
      : IsExpectedPage(page, "/batle1") || IsExpectedPage(page, "/battle1");
  }

  private static bool IsBattleLogPath(string path, FarmLocation location)
  {
    if (location == FarmLocation.Adventure)
    {
      return path.StartsWith("/domp1/log/", StringComparison.OrdinalIgnoreCase);
    }

    return path.StartsWith("/batle1/log/", StringComparison.OrdinalIgnoreCase)
           || path.StartsWith("/battle1/log/", StringComparison.OrdinalIgnoreCase);
  }
}
