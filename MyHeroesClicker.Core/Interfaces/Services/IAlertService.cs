using MyHeroesClicker.Core.Models.Alert;

namespace MyHeroesClicker.Core.Interfaces.Services;

public interface IAlertService
{
  Task PublishAsync(
    AlertEventKind kind,
    string message,
    CancellationToken cancellationToken);
}
