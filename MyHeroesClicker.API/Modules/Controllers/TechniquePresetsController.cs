using Microsoft.AspNetCore.Mvc;
using MyHeroesClicker.Core.Models.TechniquePreset;
using MyHeroesClicker.Core.Interfaces.Repositories;
using NLog;

namespace MyHeroesClicker.API.Modules.Controllers;

[ApiController]
[Route("api/technique-presets")]
public sealed class TechniquePresetsController : ControllerBase
{
  private readonly Logger _log = LogManager.GetCurrentClassLogger();
  private readonly ITechniquePresetRepository _techniquePresets;

  public TechniquePresetsController(ITechniquePresetRepository techniquePresets)
  {
    _techniquePresets = techniquePresets;
  }

  [HttpGet]
  public async Task<ActionResult<IReadOnlyCollection<TechniquePresetResponse>>> GetAllAsync(
    [FromQuery] long? accountId,
    [FromQuery] string? kind,
    CancellationToken cancellationToken)
  {
    if (!ValidateKind(kind, allowNull: true, out var validationResult))
    {
      return validationResult;
    }

    return Ok(await _techniquePresets.GetAllAsync(accountId, kind, cancellationToken));
  }

  [HttpGet("{id:long}")]
  public async Task<ActionResult<TechniquePresetResponse>> GetByIdAsync(long id, CancellationToken cancellationToken)
  {
    var preset = await _techniquePresets.GetByIdAsync(id, cancellationToken);

    return preset is null ? NotFound() : Ok(preset);
  }

  [HttpPost]
  public async Task<IActionResult> CreateAsync(
    [FromBody] CreateTechniquePresetRequest request,
    CancellationToken cancellationToken)
  {
    if (!ValidatePreset(request.Kind, out var validationResult))
    {
      return validationResult;
    }

    try
    {
      var id = await _techniquePresets.CreateAsync(request, cancellationToken);

      return CreatedAtAction(nameof(GetByIdAsync), new { id }, new { id });
    }
    catch (Exception exception)
    {
      _log.Warn(exception, "Technique preset creation failed.");

      return Conflict(new { error = exception.Message });
    }
  }

  [HttpPut("{id:long}")]
  public async Task<IActionResult> UpdateAsync(
    long id,
    [FromBody] UpdateTechniquePresetRequest request,
    CancellationToken cancellationToken)
  {
    if (!ValidatePreset(request.Kind, out var validationResult))
    {
      return validationResult;
    }

    try
    {
      var updated = await _techniquePresets.UpdateAsync(id, request, cancellationToken);

      return updated ? NoContent() : NotFound();
    }
    catch (Exception exception)
    {
      _log.Warn(exception, "Technique preset update failed.");

      return Conflict(new { error = exception.Message });
    }
  }

  [HttpDelete("{id:long}")]
  public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken)
  {
    var deleted = await _techniquePresets.DeleteAsync(id, cancellationToken);

    return deleted ? NoContent() : NotFound();
  }

  [HttpGet("{id:long}/slots")]
  public async Task<ActionResult<IReadOnlyCollection<TechniquePresetSlotResponse>>> GetSlotsAsync(
    long id,
    CancellationToken cancellationToken)
  {
    return Ok(await _techniquePresets.GetSlotsAsync(id, cancellationToken));
  }

  [HttpPut("{id:long}/slots/{techniqueNumber:int}")]
  public async Task<IActionResult> UpsertSlotAsync(
    long id,
    int techniqueNumber,
    [FromBody] UpsertTechniquePresetSlotRequest request,
    CancellationToken cancellationToken)
  {
    if (techniqueNumber is < 1 or > 15)
    {
      return BadRequest(new { error = "Номер приема должен быть в диапазоне 1..15." });
    }

    try
    {
      await _techniquePresets.UpsertSlotAsync(id, techniqueNumber, request, cancellationToken);

      return NoContent();
    }
    catch (Exception exception)
    {
      _log.Warn(exception, "Technique preset slot update failed.");

      return BadRequest(new { error = "Пресет не найден или прием указан некорректно." });
    }
  }

  [HttpDelete("{id:long}/slots/{techniqueNumber:int}")]
  public async Task<IActionResult> DeleteSlotAsync(
    long id,
    int techniqueNumber,
    CancellationToken cancellationToken)
  {
    var deleted = await _techniquePresets.DeleteSlotAsync(id, techniqueNumber, cancellationToken);

    return deleted ? NoContent() : NotFound();
  }

  private bool ValidatePreset(string kind, out IActionResult validationResult)
  {
    var errors = new Dictionary<string, string[]>();

    if (!IsValidKind(kind))
    {
      errors.Add(nameof(CreateTechniquePresetRequest.Kind), ["Допустимые значения: farm, combat."]);
    }

    validationResult = errors.Count > 0 ? BadRequest(new { errors }) : Ok();

    return errors.Count == 0;
  }

  private bool ValidateKind(string? kind, bool allowNull, out ActionResult<IReadOnlyCollection<TechniquePresetResponse>> validationResult)
  {
    if ((allowNull && kind is null) || IsValidKind(kind))
    {
      validationResult = Ok(Array.Empty<TechniquePresetResponse>());
      return true;
    }

    validationResult = BadRequest(new { error = "Допустимые значения kind: farm, combat." });
    return false;
  }

  private static bool IsValidKind(string? kind)
  {
    return string.Equals(kind, "farm", StringComparison.Ordinal)
      || string.Equals(kind, "combat", StringComparison.Ordinal);
  }
}
