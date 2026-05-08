using MyHeroesClicker.API.Contracts.Database;

namespace MyHeroesClicker.API.Core.Interfaces.Repositories;

public interface IAppSettingsRepository
{
  Task<AppSettingsResponse?> GetAsync(CancellationToken cancellationToken);

  Task<bool> UpdateAsync(UpdateAppSettingsRequest request, CancellationToken cancellationToken);

  Task<bool> SetActiveAccountAsync(long? accountId, CancellationToken cancellationToken);
}
