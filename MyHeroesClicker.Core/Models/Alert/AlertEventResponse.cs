namespace MyHeroesClicker.Core.Models.Alert;

public sealed class AlertEventResponse
{
  public AlertEventResponse(long id, string kind, string message, DateTimeOffset createdAt)
  {
    Id = id;
    Kind = kind;
    Message = message;
    CreatedAt = createdAt;
  }

  public long Id { get; }

  public string Kind { get; }

  public string Message { get; }

  public DateTimeOffset CreatedAt { get; }
}
