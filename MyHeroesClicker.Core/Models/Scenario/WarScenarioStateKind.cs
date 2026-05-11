namespace MyHeroesClicker.Core.Models.Scenario;

/// <summary>
/// Тип распознанного состояния сценария войны.
/// </summary>
public enum WarScenarioStateKind
{
  /// <summary>
  /// Активная война не найдена.
  /// </summary>
  Inactive,

  /// <summary>
  /// Активная война найдена, но состояние неизвестно.
  /// </summary>
  ActiveUnknown,

  /// <summary>
  /// Атака доступна.
  /// </summary>
  AttackAvailable,

  /// <summary>
  /// Бой находится в кулдауне.
  /// </summary>
  BattleCooldown,

  /// <summary>
  /// Страница боя доступна.
  /// </summary>
  FightPageAvailable,

  /// <summary>
  /// Бой уже идет.
  /// </summary>
  FightInProgress,

  /// <summary>
  /// Регистрация на войну доступна.
  /// </summary>
  RegistrationAvailable
}