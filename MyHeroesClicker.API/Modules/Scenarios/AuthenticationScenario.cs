using MyHeroesClicker.Browser.Browser;
using MyHeroesClicker.Core.Contracts.Database;
using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.API.Modules.Scenarios;

public sealed class AuthenticationScenario : IScenario
{
  private readonly AccountCredentialsResponse _account;
  private readonly MyHeroesWebClient _webClient;

  public AuthenticationScenario(
    AccountCredentialsResponse account,
    MyHeroesWebClient webClient)
  {
    _account = account;
    _webClient = webClient;
  }

  public string Name => "Авторизация";

  public async Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    context.Logger.Log("Выполняю авторизацию прямым запросом.");

    await _webClient.PostFormExpectedAsync(
      "/main/login",
      new Dictionary<string, string>
      {
        ["login"] = _account.Login,
        ["password"] = _account.Password!,
        ["btn_login"] = "вход"
      },
      cancellationToken);

  }
}
