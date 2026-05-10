namespace MyHeroesClicker.Core.Interfaces.Services;

public interface IAlertService
{
  Task PublishAsync(
    AlertEventKind kind,
    string message,
    CancellationToken cancellationToken);
}

public enum AlertEventKind
{
  Captcha,
  AuthenticationRequired,
  FatalError
}

public sealed record AlertEvent(
  long Id,
  AlertEventKind Kind,
  string Message,
  DateTimeOffset CreatedAt);
