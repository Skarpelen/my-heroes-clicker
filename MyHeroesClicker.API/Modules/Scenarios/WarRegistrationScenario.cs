using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using MyHeroesClicker.Browser.Browser;
using MyHeroesClicker.Core.Confs;
using MyHeroesClicker.Core.Interfaces.Scenarios;
using MyHeroesClicker.Core.Models.Scenarios;
using MyHeroesClicker.Core.Modules.Core;

namespace MyHeroesClicker.API.Modules.Scenarios;

public sealed partial class WarRegistrationScenario : IScenario
{
  private static readonly TimeSpan BattleCooldown = TimeSpan.FromHours(4);
  private static readonly TimeSpan DefaultRegistrationWindow = TimeSpan.FromMinutes(20);
  private readonly MyHeroesWebClient _webClient;
  private readonly IScenario _combatPreparationScenario;
  private readonly WarModeConfiguration _configuration;

  public WarRegistrationScenario(
    MyHeroesWebClient webClient,
    IScenario combatPreparationScenario,
    WarModeConfiguration configuration)
  {
    _webClient = webClient;
    _combatPreparationScenario = combatPreparationScenario;
    _configuration = configuration;
  }

  public string Name => "Авто война";

  public async Task ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken)
  {
    var checkInterval = GetCheckInterval(_configuration.CheckIntervalMinutes);

    while (true)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var warDocument = await _webClient.GetDocumentAsync("/clan/war", cancellationToken);
      var state = ReadWarState(warDocument);
      var now = DateTimeOffset.UtcNow;

      if (state.HasRegistration)
      {
        await PrepareAndRegisterAsync(context, state, cancellationToken);
        await DelayUntilNextCheckAsync(context, state.WarEndsAt, now + DefaultRegistrationWindow + BattleCooldown, checkInterval, cancellationToken);
        continue;
      }

      if (state.NextBattleAt is not null)
      {
        context.SetStatus("Кулдаун на сражение", state.NextBattleAt.Value);
        context.Logger.Log($"Война: кулдаун до {FormatLocalTime(state.NextBattleAt.Value)}.");
        await DelayUntilNextCheckAsync(context, state.WarEndsAt, state.NextBattleAt.Value, checkInterval, cancellationToken);
        continue;
      }

      if (state.HasFight)
      {
        await TryRegisterFromFightAsync(context, state.WarEndsAt, checkInterval, cancellationToken);
        continue;
      }

      if (state.HasAvailableAttack)
      {
        context.SetStatus("Атака доступна");
        context.Logger.Log($"Война: атака доступна, жду фазу регистрации {FormatDuration(checkInterval)}.");
        await DelayUntilNextCheckAsync(context, state.WarEndsAt, DateTimeOffset.UtcNow + checkInterval, checkInterval, cancellationToken);
        continue;
      }

      if (state.WarEndsAt is not null && state.WarEndsAt > now)
      {
        context.SetStatus("Активная война, состояние не распознано", now + checkInterval);
        context.Logger.Log("Война активна, но известное состояние не найдено. Проверю позже.");
        await DelayUntilNextCheckAsync(context, state.WarEndsAt, now + checkInterval, checkInterval, cancellationToken);
        continue;
      }

      context.SetStatus("Активная война не найдена", now + checkInterval);
      context.Logger.Log($"Активная война не найдена. Следующая проверка через {FormatDuration(checkInterval)}.");
      await Task.Delay(checkInterval, cancellationToken);
    }
  }

  private async Task TryRegisterFromFightAsync(
    ScenarioContext context,
    DateTimeOffset? warEndsAt,
    TimeSpan checkInterval,
    CancellationToken cancellationToken)
  {
    var fightDocument = await _webClient.GetDocumentAsync("/clan/fight", cancellationToken);
    var fightState = ReadFightState(fightDocument);

    if (fightState.HasRegistration)
    {
      await PrepareAndRegisterAsync(context, fightState, cancellationToken);
      var nextBattleAt = DateTimeOffset.UtcNow + (fightState.RegistrationStartsIn ?? DefaultRegistrationWindow) + BattleCooldown;
      await DelayUntilNextCheckAsync(context, warEndsAt, nextBattleAt, checkInterval, cancellationToken);

      return;
    }

    context.SetStatus("Сражение идет или запись недоступна", DateTimeOffset.UtcNow + checkInterval);
    context.Logger.Log($"Война: сражение уже идет или запись недоступна. Следующая проверка через {FormatDuration(checkInterval)}.");
    await Task.Delay(checkInterval, cancellationToken);
  }

  private async Task PrepareAndRegisterAsync(
    ScenarioContext context,
    WarPageState state,
    CancellationToken cancellationToken)
  {
    context.SetStatus("Подготовка к регистрации");
    await WaitForCombatPreparationMomentAsync(context, state, cancellationToken);
    await context.Coordinator.StopGroupAsync(ScenarioConcurrencyGroup.Farm, cancellationToken);

    context.Logger.Log("Война: надеваю боевой сет перед регистрацией.");
    await _combatPreparationScenario.ExecuteAsync(context, cancellationToken);

    var response = await _webClient.TryGetExpectedAsync("/clan/regfight", cancellationToken);

    if (!response.IsExpected)
    {
      throw new InvalidOperationException($"Регистрация на войну завершилась с кодом {response.StatusCode}.");
    }

    context.CompletedIterations++;
    context.SetStatus("Персонаж зарегистрирован");
    var registrationWindow = state.RegistrationStartsIn ?? DefaultRegistrationWindow;
    context.Logger.Log($"Война: персонаж зарегистрирован. Следующая битва ожидается через {FormatDuration(registrationWindow + BattleCooldown)}.");
  }

  private async Task WaitForCombatPreparationMomentAsync(
    ScenarioContext context,
    WarPageState state,
    CancellationToken cancellationToken)
  {
    if (state.RegistrationStartsIn is null)
    {
      return;
    }

    var preparationOffset = TimeSpan.FromSeconds(_configuration.CombatPreparationSecondsBeforeRegistrationEnd);
    var delay = state.RegistrationStartsIn.Value - preparationOffset;

    if (delay <= TimeSpan.Zero)
    {
      return;
    }

    context.SetStatus("Ожидание подготовки к бою", DateTimeOffset.UtcNow + delay);
    context.Logger.Log($"Война: запись доступна, подготовка к бою через {FormatDuration(delay)}.");
    await Task.Delay(delay, cancellationToken);
  }

  private static async Task DelayUntilNextCheckAsync(
    ScenarioContext context,
    DateTimeOffset? warEndsAt,
    DateTimeOffset plannedNextCheck,
    TimeSpan fallbackCheckInterval,
    CancellationToken cancellationToken)
  {
    var now = DateTimeOffset.UtcNow;
    var nextCheck = plannedNextCheck;

    if (warEndsAt is not null && warEndsAt.Value < nextCheck)
    {
      nextCheck = warEndsAt.Value;
      context.Logger.Log($"Война закончится раньше следующей битвы: {FormatLocalTime(nextCheck)}.");
    }

    var delay = nextCheck - now;

    if (delay < TimeSpan.Zero)
    {
      delay = fallbackCheckInterval;
    }

    context.Logger.Log($"Следующая проверка войны через {FormatDuration(delay)}.");
    context.SetStatus(context.StatusMessage ?? "Ожидание следующей проверки", now + delay);
    await Task.Delay(delay, cancellationToken);
  }

  private static WarPageState ReadWarState(string html)
  {
    var text = ToPlainText(html);
    var warEndsAt = TryReadWarEnd(text);
    var nextBattleAt = TryReadTimer(text, "До следующей битвы") is { } nextBattleIn
      ? DateTimeOffset.UtcNow + nextBattleIn
      : (DateTimeOffset?)null;

    return new WarPageState(
      warEndsAt,
      nextBattleAt,
      ContainsHref(html, "/clan/atclan"),
      ContainsHref(html, "/clan/fight"),
      ContainsHref(html, "/clan/regfight"),
      TryReadTimer(text, "Начало через"));
  }

  private static WarPageState ReadFightState(string html)
  {
    var text = ToPlainText(html);

    return new WarPageState(
      null,
      null,
      false,
      true,
      ContainsHref(html, "/clan/regfight"),
      TryReadTimer(text, "Начало через"));
  }

  private static DateTimeOffset? TryReadWarEnd(string text)
  {
    var match = WarEndRegex().Match(text);

    if (!match.Success)
    {
      return null;
    }

    if (!DateTime.TryParseExact(
          match.Groups["date"].Value,
          "HH:mm:ss dd.MM.yyyy",
          CultureInfo.InvariantCulture,
          DateTimeStyles.None,
          out var dateTime))
    {
      return null;
    }

    return new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified), TimeSpan.FromHours(3));
  }

  private static TimeSpan? TryReadTimer(string text, string label)
  {
    var labelIndex = text.IndexOf(label, StringComparison.OrdinalIgnoreCase);

    if (labelIndex < 0)
    {
      return null;
    }

    var timerText = text[labelIndex..];
    var hourMinuteMatch = HourMinuteTimerRegex().Match(timerText);

    if (hourMinuteMatch.Success)
    {
      return new TimeSpan(
        TryReadInt(hourMinuteMatch.Groups["hours"].Value),
        TryReadInt(hourMinuteMatch.Groups["minutes"].Value),
        0);
    }

    var minuteSecondMatch = MinuteSecondTimerRegex().Match(timerText);

    if (!minuteSecondMatch.Success)
    {
      return null;
    }

    return new TimeSpan(
      0,
      TryReadInt(minuteSecondMatch.Groups["minutes"].Value),
      TryReadInt(minuteSecondMatch.Groups["seconds"].Value));
  }

  private static int TryReadInt(string value)
  {
    return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
      ? result
      : 0;
  }

  private static bool ContainsHref(string html, string href)
  {
    return html.Contains($"href=\"{href}\"", StringComparison.OrdinalIgnoreCase)
           || html.Contains($"href=\"{href}/\"", StringComparison.OrdinalIgnoreCase);
  }

  private static string ToPlainText(string html)
  {
    var text = TagsRegex().Replace(html, " ");
    text = WebUtility.HtmlDecode(text);

    return SpacesRegex().Replace(text, " ");
  }

  private static TimeSpan GetCheckInterval(int minutes)
  {
    return TimeSpan.FromMinutes(Math.Clamp(minutes, 1, 60));
  }

  private static string FormatDuration(TimeSpan duration)
  {
    if (duration.TotalHours >= 1)
    {
      return $"{(int)duration.TotalHours}ч {duration.Minutes}м";
    }

    return $"{Math.Max(1, (int)Math.Ceiling(duration.TotalMinutes))}м";
  }

  private static string FormatLocalTime(DateTimeOffset dateTime)
  {
    return dateTime.ToOffset(TimeSpan.FromHours(3)).ToString("HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture);
  }

  [GeneratedRegex(@"Война\s+до:\s*(?<date>\d{1,2}:\d{2}:\d{2}\s+\d{1,2}\.\d{1,2}\.\d{4})", RegexOptions.IgnoreCase)]
  private static partial Regex WarEndRegex();

  [GeneratedRegex(@"(?<hours>\d+)\s*ч\s*:\s*(?<minutes>\d+)\s*м", RegexOptions.IgnoreCase)]
  private static partial Regex HourMinuteTimerRegex();

  [GeneratedRegex(@"(?<minutes>\d+)\s*м\s*:\s*(?<seconds>\d+)", RegexOptions.IgnoreCase)]
  private static partial Regex MinuteSecondTimerRegex();

  [GeneratedRegex("<[^>]+>")]
  private static partial Regex TagsRegex();

  [GeneratedRegex(@"\s+")]
  private static partial Regex SpacesRegex();
}

internal sealed record WarPageState(
  DateTimeOffset? WarEndsAt,
  DateTimeOffset? NextBattleAt,
  bool HasAvailableAttack,
  bool HasFight,
  bool HasRegistration,
  TimeSpan? RegistrationStartsIn);
