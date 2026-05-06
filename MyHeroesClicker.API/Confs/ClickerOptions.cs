namespace MyHeroesClicker;

public sealed class ClickerOptions
{
  public string BaseUrl { get; set; } = "https://myheroes.ru/";

  public string ConfigPath { get; set; } = Path.Combine("conf", "clicker-config.json");

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

  public string AuthStatePath { get; set; } = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "MyHeroesClicker",
    "auth-state.json");
}
