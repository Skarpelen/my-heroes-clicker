using MyHeroesClicker.Browser;
using MyHeroesClicker.Confs;
using MyHeroesClicker.Core;

namespace MyHeroesClicker.Scenarios;

public sealed class AuthenticationScenario : IScenario
{
  private readonly ClickerConfig _config;
  private readonly MyHeroesWebClient _webClient;
  private readonly Func<Task>? _afterAuthenticated;

  public AuthenticationScenario(
    ClickerConfig config,
    MyHeroesWebClient webClient,
    Func<Task>? afterAuthenticated = null)
  {
    _config = config;
    _webClient = webClient;
    _afterAuthenticated = afterAuthenticated;
  }

  public string Name => "Авторизация";

  public async Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    context.Logger.Log("Выполняю авторизацию прямым запросом.");

    await _webClient.PostFormExpectedAsync(
      "/main/login",
      new Dictionary<string, string>
      {
        ["login"] = _config.Login.UserName,
        ["password"] = _config.Login.Password,
        ["btn_login"] = "вход"
      },
      cancellationToken);

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
