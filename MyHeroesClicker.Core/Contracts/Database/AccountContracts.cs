namespace MyHeroesClicker.Core.Contracts.Database;

public sealed record AccountResponse(
  long Id,
  string Login,
  bool HasPassword,
  bool IsEnabled);

public sealed record CreateAccountRequest(
  string Login,
  string? Password,
  bool IsEnabled);

public sealed record UpdateAccountRequest(
  string Login,
  string? Password,
  bool IsEnabled);

public sealed record AccountCredentialsResponse(
  long Id,
  string Login,
  string? Password,
  bool IsEnabled);
