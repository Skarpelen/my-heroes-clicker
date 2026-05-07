namespace MyHeroesClicker.Services;

public sealed class HumanDelayService : IHumanDelayService
{
  private readonly Random _random = new();
  private readonly ClickerOptions _options;

  public HumanDelayService(ClickerOptions options)
  {
    _options = options;
  }

  public async Task WaitBeforeActionAsync(CancellationToken cancellationToken)
  {
    var delay = _random.Next(_options.MinDelayMs, _options.MaxDelayMs + 1);

    await Task.Delay(delay, cancellationToken);
  }
}
