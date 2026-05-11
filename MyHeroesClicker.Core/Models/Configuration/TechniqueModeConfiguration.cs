namespace MyHeroesClicker.Core.Models.Configuration;

/// <summary>
/// Конфигурация приемов для режимов фарма и боя.
/// </summary>
public sealed class TechniqueModeConfiguration
{
  /// <summary>
  /// Идентификаторы приемов, включенных в режиме фарма.
  /// </summary>
  public IReadOnlySet<int> FarmEnabledTechniqueIds { get; init; } = new HashSet<int>();

  /// <summary>
  /// Идентификаторы приемов, включенных в режиме боя.
  /// </summary>
  public IReadOnlySet<int> CombatEnabledTechniqueIds { get; init; } = new HashSet<int>();
}