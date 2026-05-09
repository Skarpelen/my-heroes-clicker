using Microsoft.AspNetCore.Mvc;
using MyHeroesClicker.Core.Contracts.Database;
using MyHeroesClicker.Core.Interfaces.Repositories;

namespace MyHeroesClicker.API.Modules.Controllers;

[ApiController]
[Route("api/equipment-sets")]
public sealed class EquipmentSetsController : ControllerBase
{
  private readonly IEquipmentSetRepository _equipmentSets;
  private readonly ILogger<EquipmentSetsController> _logger;

  public EquipmentSetsController(
    IEquipmentSetRepository equipmentSets,
    ILogger<EquipmentSetsController> logger)
  {
    _equipmentSets = equipmentSets;
    _logger = logger;
  }

  [HttpGet]
  public async Task<ActionResult<IReadOnlyCollection<EquipmentSetResponse>>> GetAllAsync(
    [FromQuery] long? accountId,
    [FromQuery] string? kind,
    CancellationToken cancellationToken)
  {
    if (!ValidateKind(kind, allowNull: true, out var validationResult))
    {
      return validationResult;
    }

    return Ok(await _equipmentSets.GetAllAsync(accountId, kind, cancellationToken));
  }

  [HttpGet("{id:long}")]
  public async Task<ActionResult<EquipmentSetResponse>> GetByIdAsync(long id, CancellationToken cancellationToken)
  {
    var set = await _equipmentSets.GetByIdAsync(id, cancellationToken);

    return set is null ? NotFound() : Ok(set);
  }

  [HttpPost]
  public async Task<IActionResult> CreateAsync(
    [FromBody] CreateEquipmentSetRequest request,
    CancellationToken cancellationToken)
  {
    if (!ValidateSet(request.Kind, out var validationResult))
    {
      return validationResult;
    }

    try
    {
      var id = await _equipmentSets.CreateAsync(request, cancellationToken);

      return CreatedAtAction(nameof(GetByIdAsync), new { id }, new { id });
    }
    catch (Exception exception)
    {
      _logger.LogWarning(exception, "Equipment set creation failed.");

      return Conflict(new { error = exception.Message });
    }
  }

  [HttpPut("{id:long}")]
  public async Task<IActionResult> UpdateAsync(
    long id,
    [FromBody] UpdateEquipmentSetRequest request,
    CancellationToken cancellationToken)
  {
    if (!ValidateSet(request.Kind, out var validationResult))
    {
      return validationResult;
    }

    try
    {
      var updated = await _equipmentSets.UpdateAsync(id, request, cancellationToken);

      return updated ? NoContent() : NotFound();
    }
    catch (Exception exception)
    {
      _logger.LogWarning(exception, "Equipment set update failed.");

      return Conflict(new { error = exception.Message });
    }
  }

  [HttpDelete("{id:long}")]
  public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken)
  {
    var deleted = await _equipmentSets.DeleteAsync(id, cancellationToken);

    return deleted ? NoContent() : NotFound();
  }

  [HttpGet("{id:long}/slots")]
  public async Task<ActionResult<IReadOnlyCollection<EquipmentSetSlotResponse>>> GetSlotsAsync(
    long id,
    CancellationToken cancellationToken)
  {
    return Ok(await _equipmentSets.GetSlotsAsync(id, cancellationToken));
  }

  [HttpPut("{id:long}/slots/{slotNumber:int}")]
  public async Task<IActionResult> UpsertSlotAsync(
    long id,
    int slotNumber,
    [FromBody] UpsertEquipmentSetSlotRequest request,
    CancellationToken cancellationToken)
  {
    if (slotNumber <= 0)
    {
      return BadRequest(new { error = "Номер слота должен быть положительным." });
    }

    try
    {
      await _equipmentSets.UpsertSlotAsync(id, slotNumber, request, cancellationToken);

      return NoContent();
    }
    catch (Exception exception)
    {
      _logger.LogWarning(exception, "Equipment set slot update failed.");

      return BadRequest(new { error = "Сет не найден или слот указан некорректно." });
    }
  }

  [HttpDelete("{id:long}/slots/{slotNumber:int}")]
  public async Task<IActionResult> DeleteSlotAsync(
    long id,
    int slotNumber,
    CancellationToken cancellationToken)
  {
    var deleted = await _equipmentSets.DeleteSlotAsync(id, slotNumber, cancellationToken);

    return deleted ? NoContent() : NotFound();
  }

  private bool ValidateSet(string kind, out IActionResult validationResult)
  {
    var errors = new Dictionary<string, string[]>();

    if (!IsValidKind(kind))
    {
      errors.Add(nameof(CreateEquipmentSetRequest.Kind), ["Допустимые значения: farm, combat."]);
    }

    validationResult = errors.Count > 0 ? BadRequest(new { errors }) : Ok();

    return errors.Count == 0;
  }

  private bool ValidateKind(string? kind, bool allowNull, out ActionResult<IReadOnlyCollection<EquipmentSetResponse>> validationResult)
  {
    if ((allowNull && kind is null) || IsValidKind(kind))
    {
      validationResult = Ok(Array.Empty<EquipmentSetResponse>());
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
