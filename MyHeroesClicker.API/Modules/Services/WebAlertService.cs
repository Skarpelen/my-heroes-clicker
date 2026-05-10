using System.Threading.Channels;
using MyHeroesClicker.Core.Interfaces.Services;

namespace MyHeroesClicker.API.Modules.Services;

public sealed class WebAlertService : IAlertService
{
  private readonly object _sync = new();
  private readonly List<Channel<AlertEvent>> _subscribers = [];
  private long _lastEventId;

  public Task PublishAsync(
    AlertEventKind kind,
    string message,
    CancellationToken cancellationToken)
  {
    var alertEvent = new AlertEvent(
      Interlocked.Increment(ref _lastEventId),
      kind,
      message,
      DateTimeOffset.UtcNow);

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
