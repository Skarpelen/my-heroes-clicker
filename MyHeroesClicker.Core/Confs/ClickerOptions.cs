namespace MyHeroesClicker.Core.Confs;

public sealed class ClickerOptions
{
  public string BaseUrl { get; set; } = "https://myheroes.ru/";

  public string BrowserKind { get; set; } = "chrome";

  public bool Headless { get; set; }

  public int MinDelayMs { get; set; } = 100;

  public int MaxDelayMs { get; set; } = 250;

  public int DefaultTimeoutMs { get; set; } = 10000;

  public int HpRecoveryDelayMultiplier { get; set; } = 20;

  public double MinAttackHealthPercent { get; set; } = 0.25;

  public double MaxAttackHealthPercent { get; set; } = 0.30;

  public int MaxStepRetryCount { get; set; } = 10;

  public int RetryDelayMs { get; set; } = 1000;

  public int AuthenticationRetryDelayMs { get; set; } = 60000;

  public string UserDataDir { get; set; } = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "MyHeroesClicker",
    "chrome-profile");
}
