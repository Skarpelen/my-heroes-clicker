namespace MyHeroesClicker.Core.Models.Configuration;

/// <summary>
/// Конфигурация выполнения сценариев.
/// </summary>
public sealed class ScenarioConfiguration
{
  /// <summary>
  /// Конфигурацию режимов экипировки.
  /// </summary>
  public EquipmentModeConfiguration Equipment { get; init; } = new EquipmentModeConfiguration { };

  /// <summary>
  /// Конфигурацию режимов приемов.
  /// </summary>
  public TechniqueModeConfiguration Techniques { get; init; } = new TechniqueModeConfiguration { };

  /// <summary>
  /// Конфигурацию режима войны.
  /// </summary>
  public WarModeConfiguration War { get; init; } = new WarModeConfiguration { };
}
