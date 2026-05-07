namespace MyHeroesClicker.Core;

public sealed class CharacterState
{
  public int MaxHealth { get; private set; }

  public void UpdateMaxHealth(int maxHealth)
  {
    if (maxHealth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxHealth), "Максимальное здоровье должно быть положительным.");
    }

    MaxHealth = maxHealth;
  }
}
