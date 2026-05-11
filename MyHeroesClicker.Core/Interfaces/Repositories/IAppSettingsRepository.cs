using MyHeroesClicker.Core.Models.AppSetting;

namespace MyHeroesClicker.Core.Interfaces.Repositories;

public interface IAppSettingsRepository
{
  Task<AppSettingsResponse?> GetAsync(CancellationToken cancellationToken);

  Task<bool> UpdateAsync(UpdateAppSettingsRequest request, CancellationToken cancellationToken);

  Task<bool> SetActiveAccountAsync(long? accountId, CancellationToken cancellationToken);
}
