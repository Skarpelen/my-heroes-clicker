namespace MyHeroesClicker.Core.Contracts.Database;

public sealed record AccountResponse(
  long Id,
  string Login,
  string? EncryptedPassword,
  bool IsEnabled);

public sealed record CreateAccountRequest(
  string Login,
  string? EncryptedPassword,
  bool IsEnabled);

public sealed record UpdateAccountRequest(
  string Login,
  string? EncryptedPassword,
  bool IsEnabled);
