namespace MyHeroesClicker.Core.Models.TechniquePreset;

/// <summary>
/// Данные слота пресета приемов, возвращаемые клиентам API.
/// </summary>
public sealed class TechniquePresetSlotResponse
{
  /// <summary>
  /// Идентификатор пресета приемов.
  /// </summary>
  public long TechniquePresetId { get; init; }

  /// <summary>
  /// Номер приема.
  /// </summary>
  public int TechniqueNumber { get; init; }

  /// <summary>
  /// Название приема.
  /// </summary>
  public string TechniqueName { get; init; } = string.Empty;

  /// <summary>
  /// Включен ли прием.
  /// </summary>
  public bool IsEnabled { get; init; }
}