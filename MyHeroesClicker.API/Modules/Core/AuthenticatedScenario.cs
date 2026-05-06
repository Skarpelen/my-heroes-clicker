namespace MyHeroesClicker.Core;

public sealed class AuthenticatedScenario : IScenario
{
  private readonly IScenario _authenticationScenario;
  private readonly IScenario _innerScenario;

  public AuthenticatedScenario(IScenario authenticationScenario, IScenario innerScenario)
  {
    _authenticationScenario = authenticationScenario;
    _innerScenario = innerScenario;
  }

  public string Name => _innerScenario.Name;

  public async Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    context.Logger.Log($"Выполняется базовый сценарий: {_authenticationScenario.Name}");
    await _authenticationScenario.ExecuteAsync(context, cancellationToken);

    await _innerScenario.ExecuteAsync(context, cancellationToken);
  }
}
