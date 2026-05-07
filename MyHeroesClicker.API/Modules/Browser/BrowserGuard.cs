using Microsoft.Playwright;
using MyHeroesClicker.Core;
using MyHeroesClicker.Diagnostics;
using MyHeroesClicker.Services;

namespace MyHeroesClicker.Browser;

public sealed class BrowserGuard
{
  private readonly IAlertService _alertService;
  private readonly FailureDumpService _failureDumpService;

  public BrowserGuard(IAlertService alertService, FailureDumpService failureDumpService)
  {
    _alertService = alertService;
    _failureDumpService = failureDumpService;
  }

  public async Task<bool> IsBattlePageAsync(IPage page)
  {
    if (await IsLoginPageAsync(page))
    {
      return false;
    }

    if (!IsExpectedPage(page, "/batle1") && !IsExpectedPage(page, "/battle1"))
    {
      return false;
    }

    return await BattlePageLocators.AttackButton(page).CountAsync() > 0;
  }

  public async Task<bool> IsBattleLogPageAsync(IPage page)
  {
    if (await IsLoginPageAsync(page))
    {
      return false;
    }

    if (!Uri.TryCreate(page.Url, UriKind.Absolute, out var uri)
        || uri.Scheme != "https"
        || uri.Host != "myheroes.ru"
        || !IsBattleLogPath(uri.AbsolutePath))
    {
      return false;
    }

    return await BattlePageLocators.ReturnToBattleButton(page).CountAsync() > 0;
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

  public async Task ExpectBattlePageAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    var page = context.Page;

    if (!await IsBattlePageAsync(page))
    {
      await StopWithErrorAsync(context, $"Ожидалась страница боя, но текущий URL: {context.Page.Url}", cancellationToken);
    }

    var attackButton = BattlePageLocators.AttackButton(page);

    if (await attackButton.CountAsync() == 0)
    {
      await StopWithErrorAsync(context, "Не найдена кнопка атаки x10.", cancellationToken);
    }
  }

  public async Task ExpectBattleLogPageAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    var page = context.Page;

    if (!await IsBattleLogPageAsync(page))
    {
      await StopWithErrorAsync(context, $"Ожидалась страница логов, но текущий URL: {context.Page.Url}", cancellationToken);
    }

    var returnButton = BattlePageLocators.ReturnToBattleButton(page);

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

    context.Logger.Log("Действие устарело. Обновляю страницу и повторяю шаг.");

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

  private static bool IsBattleLogPath(string path)
  {
    return path.StartsWith("/batle1/log/", StringComparison.OrdinalIgnoreCase)
           || path.StartsWith("/battle1/log/", StringComparison.OrdinalIgnoreCase);
  }
}
