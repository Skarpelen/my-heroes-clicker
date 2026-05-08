using Microsoft.AspNetCore.Mvc;
using MyHeroesClicker.Core.Contracts.Database;
using MyHeroesClicker.Core.Interfaces.Repositories;

namespace MyHeroesClicker.API.Modules.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountsController : ControllerBase
{
  private readonly IAccountRepository _accounts;
  private readonly ILogger<AccountsController> _logger;

  public AccountsController(
    IAccountRepository accounts,
    ILogger<AccountsController> logger)
  {
    _accounts = accounts;
    _logger = logger;
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
    if (!ValidateAccount(request.Title, request.Login, out var validationResult))
    {
      return validationResult;
    }

    try
    {
      var id = await _accounts.CreateAsync(request, cancellationToken);

      return CreatedAtAction(nameof(GetByIdAsync), new { id }, new { id });
    }
    catch (Exception exception)
    {
      _logger.LogWarning(exception, "Account creation failed.");

      return Conflict(new { error = exception.Message });
    }
  }

  [HttpPut("{id:long}")]
  public async Task<IActionResult> UpdateAsync(
    long id,
    [FromBody] UpdateAccountRequest request,
    CancellationToken cancellationToken)
  {
    if (!ValidateAccount(request.Title, request.Login, out var validationResult))
    {
      return validationResult;
    }

    var updated = await _accounts.UpdateAsync(id, request, cancellationToken);

    return updated ? NoContent() : NotFound();
  }

  [HttpDelete("{id:long}")]
  public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken)
  {
    var deleted = await _accounts.DeleteAsync(id, cancellationToken);

    return deleted ? NoContent() : NotFound();
  }

  private bool ValidateAccount(string title, string login, out IActionResult validationResult)
  {
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(title))
    {
      errors.Add(nameof(CreateAccountRequest.Title), ["Укажите название аккаунта."]);
    }

    if (string.IsNullOrWhiteSpace(login))
    {
      errors.Add(nameof(CreateAccountRequest.Login), ["Укажите логин аккаунта."]);
    }

    validationResult = errors.Count > 0 ? BadRequest(new { errors }) : Ok();

    return errors.Count == 0;
  }
}
