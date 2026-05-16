namespace MyHeroesClicker.Core.Models.AppSetting;

public sealed class UpdateAlertSoundSettingsRequest
{
  public bool Enabled { get; init; }

  public double Volume { get; init; }
}
