namespace MyHeroesClicker.Core.Models.Alert;

/// <summary>
/// Тип оповещения, создаваемого средой выполнения кликера.
/// </summary>
public enum AlertEventKind
{
  /// <summary>
  /// Обнаружена капча.
  /// </summary>
  Captcha,

  /// <summary>
  /// Для продолжения сценария требуется авторизация.
  /// </summary>
  AuthenticationRequired,

  /// <summary>
  /// Произошла критическая ошибка во время выполнения.
  /// </summary>
  FatalError
}