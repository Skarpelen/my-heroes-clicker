namespace MyHeroesClicker.Core.Confs;

public sealed class ClickerOptions
{
  public string BaseUrl { get; set; } = string.Empty;

  public string BrowserKind { get; set; } = string.Empty;

  public bool Headless { get; set; }

  public int MinDelayMs { get; set; }

  public int MaxDelayMs { get; set; }

  public int DefaultTimeoutMs { get; set; }

  public int HpRecoveryDelayMultiplier { get; set; }

  public double MinAttackHealthPercent { get; set; }

  public double MaxAttackHealthPercent { get; set; }

  public int MaxStepRetryCount { get; set; }

  public int RetryDelayMs { get; set; }

  public int AuthenticationRetryDelayMs { get; set; }

  public string UserDataDir { get; set; } = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "MyHeroesClicker",
    "chrome-profile");
}
