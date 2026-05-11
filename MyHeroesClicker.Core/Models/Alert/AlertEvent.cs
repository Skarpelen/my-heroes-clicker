namespace MyHeroesClicker.Core.Models.Alert;

public sealed class AlertEvent
{
  public AlertEvent(long id, AlertEventKind kind, string message, DateTimeOffset createdAt)
  {
    Id = id;
    Kind = kind;
    Message = message;
    CreatedAt = createdAt;
  }

  public long Id { get; }

  public AlertEventKind Kind { get; }

  public string Message { get; }

  public DateTimeOffset CreatedAt { get; }
}
