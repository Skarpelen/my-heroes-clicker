namespace MyHeroesClicker.Core.Models.Browser;

/// <summary>
/// Результат HTTP-команды, отправленной в игру.
/// </summary>
public sealed class CommandResponse
{
  /// <summary>
  /// HTTP-код состояния.
  /// </summary>
  public int StatusCode { get; init; }

  /// <summary>
  /// Вернула ли команда ожидаемый код состояния.
  /// </summary>
  public bool IsExpected { get; init; }
}