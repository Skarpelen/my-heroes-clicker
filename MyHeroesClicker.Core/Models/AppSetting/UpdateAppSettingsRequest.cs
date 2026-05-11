namespace MyHeroesClicker.Core.Models.AppSetting;

/// <summary>
/// Запрос на обновление настроек приложения.
/// </summary>
public sealed class UpdateAppSettingsRequest
{
  /// <summary>
  /// Базовый URL игры.
  /// </summary>
  public string BaseUrl { get; init; } = string.Empty;

  /// <summary>
  /// Тип браузера.
  /// </summary>
  public string BrowserKind { get; init; } = string.Empty;

  /// <summary>
  /// Запускается ли браузер в headless-режиме.
  /// </summary>
  public bool Headless { get; init; }

  /// <summary>
  /// Каталог пользовательских данных браузера.
  /// </summary>
  public string? UserDataDir { get; init; }

  /// <summary>
  /// Минимальную пользовательскую задержку в миллисекундах.
  /// </summary>
  public int MinDelayMs { get; init; }

  /// <summary>
  /// Максимальную пользовательскую задержку в миллисекундах.
  /// </summary>
  public int MaxDelayMs { get; init; }

  /// <summary>
  /// Таймаут операции по умолчанию в миллисекундах.
  /// </summary>
  public int DefaultTimeoutMs { get; init; }

  /// <summary>
  /// Множитель задержки восстановления здоровья.
  /// </summary>
  public int HpRecoveryDelayMultiplier { get; init; }

  /// <summary>
  /// Минимальный допустимый процент здоровья перед атакой.
  /// </summary>
  public double MinAttackHealthPercent { get; init; }

  /// <summary>
  /// Максимальный допустимый процент здоровья перед атакой.
  /// </summary>
  public double MaxAttackHealthPercent { get; init; }

  /// <summary>
  /// Максимальное количество повторов шага.
  /// </summary>
  public int MaxStepRetryCount { get; init; }

  /// <summary>
  /// Задержку перед повтором в миллисекундах.
  /// </summary>
  public int RetryDelayMs { get; init; }

  /// <summary>
  /// Задержку перед повторной авторизацией в миллисекундах.
  /// </summary>
  public int AuthenticationRetryDelayMs { get; init; }

  /// <summary>
  /// Интервал проверки войны в минутах.
  /// </summary>
  public int WarCheckIntervalMinutes { get; init; }

  /// <summary>
  /// Смещение подготовки к бою до окончания регистрации на войну в секундах.
  /// </summary>
  public int WarCombatPreparationSecondsBeforeRegistrationEnd { get; init; }
}