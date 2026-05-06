namespace MyHeroesClicker.Confs;

public sealed class EquipmentConfig
{
  public EquipmentStyleConfig FarmStyle { get; set; } = new();

  public EquipmentStyleConfig CombatStyle { get; set; } = new();
}
