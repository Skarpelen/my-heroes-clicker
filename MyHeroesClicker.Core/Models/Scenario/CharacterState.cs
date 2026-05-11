namespace MyHeroesClicker.Core.Models.Scenario;

/// <summary>
/// Изменяемое состояние персонажа, собранное во время выполнения сценария.
/// </summary>
public sealed class CharacterState
{
  /// <summary>
  /// Максимальное здоровье персонажа.
  /// </summary>
  public int MaxHealth { get; private set; }

  /// <summary>
  /// Обновляет максимальное здоровье персонажа.
  /// </summary>
  /// <param name="maxHealth">Максимальное значение здоровья.</param>
  public void UpdateMaxHealth(int maxHealth)
  {
    if (maxHealth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxHealth), "Максимальное здоровье должно быть положительным.");
    }

    MaxHealth = maxHealth;
  }
}