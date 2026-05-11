namespace MyHeroesClicker.Core.Models.TechniquePreset;

/// <summary>
/// Запрос на создание или обновление слота пресета приемов.
/// </summary>
public sealed class UpsertTechniquePresetSlotRequest
{
  /// <summary>
  /// Название приема.
  /// </summary>
  public string TechniqueName { get; init; } = string.Empty;

  /// <summary>
  /// Включен ли прием.
  /// </summary>
  public bool IsEnabled { get; init; }
}