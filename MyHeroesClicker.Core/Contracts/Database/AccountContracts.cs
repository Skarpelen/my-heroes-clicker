namespace MyHeroesClicker.API.Contracts.Database;

public sealed record AccountResponse(
  long Id,
  string Title,
  string Login,
  string? EncryptedPassword,
  string? AuthStatePath,
  bool IsEnabled);

public sealed record CreateAccountRequest(
  string Title,
  string Login,
  string? EncryptedPassword,
  string? AuthStatePath,
  bool IsEnabled);

public sealed record UpdateAccountRequest(
  string Title,
  string Login,
  string? EncryptedPassword,
  string? AuthStatePath,
  bool IsEnabled);
