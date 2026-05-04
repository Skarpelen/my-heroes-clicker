using Microsoft.Playwright;

namespace MyHeroesClicker.Browser;

public sealed class BattleResourcesReader
{
  public async Task<int> ReadCurrentHealthAsync(IPage page)
  {
    var healthText = await BattlePageLocators.Health(page).InnerTextAsync();
    var digits = new string(healthText.Where(char.IsDigit).ToArray());

    if (!int.TryParse(digits, out var health))
    {
      throw new InvalidOperationException($"Не удалось прочитать текущее здоровье из текста: {healthText}");
    }

    return health;
  }

  public async Task<int> ReadRequiredZealAsync(ILocator warning)
  {
    var warningText = await warning.InnerTextAsync();
    var digits = new string(warningText.Where(char.IsDigit).ToArray());

    if (!int.TryParse(digits, out var requiredZeal))
    {
      throw new InvalidOperationException($"Не удалось прочитать требуемое рвение из текста: {warningText}");
    }

    return requiredZeal;
  }
}
