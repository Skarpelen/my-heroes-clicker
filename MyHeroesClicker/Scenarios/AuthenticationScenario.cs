using Microsoft.Playwright;
using MyHeroesClicker.Browser;
using MyHeroesClicker.Confs;
using MyHeroesClicker.Core;

namespace MyHeroesClicker.Scenarios;

public sealed class AuthenticationScenario : IScenario
{
  private readonly ClickerConfig _config;
  private readonly Func<Task>? _afterAuthenticated;

  public AuthenticationScenario(ClickerConfig config, Func<Task>? afterAuthenticated = null)
  {
    _config = config;
    _afterAuthenticated = afterAuthenticated;
  }

  public string Name => "Авторизация";

  public async Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    var page = context.Page;

    await page.GotoAsync(context.Options.BaseUrl, new()
    {
      WaitUntil = WaitUntilState.DOMContentLoaded
    });

    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    if (!await context.Guard.IsLoginPageAsync(page))
    {
      context.Logger.Log("Сессия уже авторизована.");
      await SaveAuthStateAsync();

      return;
    }

    context.Logger.Log("Выполняю авторизацию.");

    await LoginPageLocators.LoginInput(page).FillAsync(_config.Login.UserName);
    await LoginPageLocators.PasswordInput(page).FillAsync(_config.Login.Password);

    var submitButton = LoginPageLocators.SubmitButton(page);

    await context.PageInteractor.PrepareForClickAsync(context, submitButton, cancellationToken);
    await submitButton.ClickAsync();
    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    if (await context.Guard.IsLoginPageAsync(page))
    {
      throw new InvalidOperationException("Не удалось авторизоваться. Проверьте логин и пароль.");
    }

    await SaveAuthStateAsync();
  }

  private async Task SaveAuthStateAsync()
  {
    if (_afterAuthenticated is not null)
    {
      await _afterAuthenticated();
    }
  }
}
