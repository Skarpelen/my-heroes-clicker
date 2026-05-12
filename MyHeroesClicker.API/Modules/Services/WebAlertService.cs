using System.Threading.Channels;
using MyHeroesClicker.Core.Interfaces.Services;
using MyHeroesClicker.Core.Models.Alert;

namespace MyHeroesClicker.API.Modules.Services;

public sealed class WebAlertService : IAlertService
{
  private readonly AlertSoundService _soundService;
  private readonly object _sync = new();
  private readonly List<Channel<AlertEvent>> _subscribers = [];
  private long _lastEventId;

  public WebAlertService(AlertSoundService soundService)
  {
    _soundService = soundService;
  }

  public Task PublishAsync(
    AlertEventKind kind,
    string message,
    CancellationToken cancellationToken)
  {
    var alertEvent = new AlertEvent
    {
      Id = Interlocked.Increment(ref _lastEventId),
      Kind = kind,
      Message = message,
      CreatedAt = DateTimeOffset.UtcNow
    };

    _soundService.Play(kind);

    Channel<AlertEvent>[] subscribers;

    lock (_sync)
    {
      subscribers = _subscribers.ToArray();
    }

    foreach (var subscriber in subscribers)
    {
      subscriber.Writer.TryWrite(alertEvent);
    }

    return Task.CompletedTask;
  }

  public ChannelReader<AlertEvent> Subscribe(CancellationToken cancellationToken)
  {
    var channel = Channel.CreateUnbounded<AlertEvent>(new UnboundedChannelOptions
    {
      SingleReader = true,
      SingleWriter = false
    });

    lock (_sync)
    {
      _subscribers.Add(channel);
    }

    cancellationToken.Register(() =>
    {
      lock (_sync)
      {
        _subscribers.Remove(channel);
      }

      channel.Writer.TryComplete();
    });

    return channel.Reader;
  }
}
