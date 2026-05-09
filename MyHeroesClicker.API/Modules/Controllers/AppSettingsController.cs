using Microsoft.AspNetCore.Mvc;
using MyHeroesClicker.Core.Contracts.Database;
using MyHeroesClicker.Core.Interfaces.Repositories;
using NLog;

namespace MyHeroesClicker.API.Modules.Controllers;

[ApiController]
[Route("api/settings")]
public sealed class AppSettingsController : ControllerBase
{
  private readonly Logger _log = LogManager.GetCurrentClassLogger();
  private readonly IAppSettingsRepository _settings;

  public AppSettingsController(IAppSettingsRepository settings)
  {
    _settings = settings;
  }

  [HttpGet]
  public async Task<ActionResult<AppSettingsResponse>> GetAsync(CancellationToken cancellationToken)
  {
    var settings = await _settings.GetAsync(cancellationToken);

    return settings is null ? NotFound() : Ok(settings);
  }

  [HttpPut]
  public async Task<IActionResult> UpdateAsync(
    [FromBody] UpdateAppSettingsRequest request,
    CancellationToken cancellationToken)
  {
    if (!ValidateSettings(request, out var validationResult))
    {
      return validationResult;
    }

    var updated = await _settings.UpdateAsync(request, cancellationToken);

    return updated ? NoContent() : NotFound();
  }

  [HttpPut("active-account")]
  public async Task<IActionResult> SetActiveAccountAsync(
    [FromBody] SetActiveAccountRequest request,
    CancellationToken cancellationToken)
  {
    try
    {
      var updated = await _settings.SetActiveAccountAsync(request.AccountId, cancellationToken);

      return updated ? NoContent() : NotFound();
    }
    catch (Exception exception)
    {
      _log.Warn(exception, "Active account update failed.");

      return BadRequest(new { error = "Указанный аккаунт не найден." });
    }
  }

  private bool ValidateSettings(UpdateAppSettingsRequest request, out IActionResult validationResult)
  {
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(request.BaseUrl))
    {
      errors.Add(nameof(UpdateAppSettingsRequest.BaseUrl), ["Укажите базовый URL."]);
    }

    if (!IsValidBrowserKind(request.BrowserKind))
    {
      errors.Add(nameof(UpdateAppSettingsRequest.BrowserKind), ["Допустимые значения: chromium, chrome, edge, firefox, webkit."]);
    }

    if (request.MinDelayMs < 0 || request.MaxDelayMs < request.MinDelayMs)
    {
      errors.Add(nameof(UpdateAppSettingsRequest.MaxDelayMs), ["Диапазон задержки указан некорректно."]);
    }

    if (request.DefaultTimeoutMs <= 0)
    {
      errors.Add(nameof(UpdateAppSettingsRequest.DefaultTimeoutMs), ["Таймаут должен быть положительным."]);
    }

    if (request.MaxStepRetryCount < 0)
    {
      errors.Add(nameof(UpdateAppSettingsRequest.MaxStepRetryCount), ["Количество повторов не должно быть отрицательным."]);
    }

    validationResult = errors.Count > 0 ? BadRequest(new { errors }) : Ok();

    return errors.Count == 0;
  }

  private static bool IsValidBrowserKind(string? browserKind)
  {
    return string.Equals(browserKind, "chromium", StringComparison.Ordinal)
      || string.Equals(browserKind, "chrome", StringComparison.Ordinal)
      || string.Equals(browserKind, "edge", StringComparison.Ordinal)
      || string.Equals(browserKind, "firefox", StringComparison.Ordinal)
      || string.Equals(browserKind, "webkit", StringComparison.Ordinal);
  }
}
