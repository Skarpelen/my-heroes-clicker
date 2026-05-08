using MyHeroesClicker.API.Contracts.Database;

namespace MyHeroesClicker.API.Core.Interfaces.Repositories;

public interface ITechniquePresetRepository
{
  Task<IReadOnlyCollection<TechniquePresetResponse>> GetAllAsync(
    long? accountId,
    string? kind,
    CancellationToken cancellationToken);

  Task<TechniquePresetResponse?> GetByIdAsync(long id, CancellationToken cancellationToken);

  Task<long> CreateAsync(CreateTechniquePresetRequest request, CancellationToken cancellationToken);

  Task<bool> UpdateAsync(long id, UpdateTechniquePresetRequest request, CancellationToken cancellationToken);

  Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);

  Task<IReadOnlyCollection<TechniquePresetSlotResponse>> GetSlotsAsync(
    long techniquePresetId,
    CancellationToken cancellationToken);

  Task UpsertSlotAsync(
    long techniquePresetId,
    int techniqueNumber,
    UpsertTechniquePresetSlotRequest request,
    CancellationToken cancellationToken);

  Task<bool> DeleteSlotAsync(long techniquePresetId, int techniqueNumber, CancellationToken cancellationToken);
}
