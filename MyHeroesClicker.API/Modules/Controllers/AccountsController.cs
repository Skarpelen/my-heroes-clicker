using Microsoft.AspNetCore.Mvc;
using MyHeroesClicker.Core.Models.Account;
using MyHeroesClicker.Core.Interfaces.Repositories;
using NLog;

namespace MyHeroesClicker.API.Modules.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountsController : ControllerBase
{
  private readonly Logger _log = LogManager.GetCurrentClassLogger();
  private readonly IAccountRepository _accounts;

  public AccountsController(IAccountRepository accounts)
  {
    _accounts = accounts;
  }

  [HttpGet]
  public async Task<ActionResult<IReadOnlyCollection<AccountResponse>>> GetAllAsync(CancellationToken cancellationToken)
  {
    return Ok(await _accounts.GetAllAsync(cancellationToken));
  }

  [HttpGet("{id:long}")]
  public async Task<ActionResult<AccountResponse>> GetByIdAsync(long id, CancellationToken cancellationToken)
  {
    var account = await _accounts.GetByIdAsync(id, cancellationToken);

    return account is null ? NotFound() : Ok(account);
  }

  [HttpPost]
  public async Task<IActionResult> CreateAsync(
    [FromBody] CreateAccountRequest request,
    CancellationToken cancellationToken)
  {
    if (!ValidateAccount(request.Login, out var validationResult))
    {
      return validationResult;
    }

    try
    {
      var id = await _accounts.CreateAsync(request, cancellationToken);

      return CreatedAtAction(nameof(GetByIdAsync), new { id }, new { id });
    }
    catch (Exception exception) when (IsUniqueConstraintViolation(exception))
    {
      _log.Warn(exception, "Account creation failed: duplicate login.");

      return Conflict(new { error = "Аккаунт с таким логином уже существует." });
    }
    catch (Exception exception)
    {
      _log.Warn(exception, "Account creation failed.");

      return Conflict(new { error = exception.Message });
    }
  }

  [HttpPut("{id:long}")]
  public async Task<IActionResult> UpdateAsync(
    long id,
    [FromBody] UpdateAccountRequest request,
    CancellationToken cancellationToken)
  {
    if (!ValidateAccount(request.Login, out var validationResult))
    {
      return validationResult;
    }

    try
    {
      var updated = await _accounts.UpdateAsync(id, request, cancellationToken);

      return updated ? NoContent() : NotFound();
    }
    catch (Exception exception) when (IsUniqueConstraintViolation(exception))
    {
      _log.Warn(exception, "Account update failed: duplicate login.");

      return Conflict(new { error = "Аккаунт с таким логином уже существует." });
    }
  }

  [HttpDelete("{id:long}")]
  public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken)
  {
    var deleted = await _accounts.DeleteAsync(id, cancellationToken);

    return deleted ? NoContent() : NotFound();
  }

  private bool ValidateAccount(string login, out IActionResult validationResult)
  {
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(login))
    {
      errors.Add(nameof(CreateAccountRequest.Login), ["Укажите логин аккаунта."]);
    }

    validationResult = errors.Count > 0 ? BadRequest(new { errors }) : Ok();

    return errors.Count == 0;
  }

  private static bool IsUniqueConstraintViolation(Exception exception)
  {
    return exception.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase);
  }
}
