namespace MyHeroesClicker.Confs;

public sealed class ClickerConfig
{
  public LoginConfig Login { get; set; } = new();

  public EquipmentConfig Equipment { get; set; } = new();

  public TechniquesConfig Techniques { get; set; } = new();
}
