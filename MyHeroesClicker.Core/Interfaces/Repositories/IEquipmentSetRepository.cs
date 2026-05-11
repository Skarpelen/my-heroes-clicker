using MyHeroesClicker.Core.Models.EquipmentSet;

namespace MyHeroesClicker.Core.Interfaces.Repositories;

public interface IEquipmentSetRepository
{
  Task<IReadOnlyCollection<EquipmentSetResponse>> GetAllAsync(
    long? accountId,
    string? kind,
    CancellationToken cancellationToken);

  Task<EquipmentSetResponse?> GetByIdAsync(long id, CancellationToken cancellationToken);

  Task<long> CreateAsync(CreateEquipmentSetRequest request, CancellationToken cancellationToken);

  Task<bool> UpdateAsync(long id, UpdateEquipmentSetRequest request, CancellationToken cancellationToken);

  Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);

  Task<IReadOnlyCollection<EquipmentSetSlotResponse>> GetSlotsAsync(
    long equipmentSetId,
    CancellationToken cancellationToken);

  Task UpsertSlotAsync(
    long equipmentSetId,
    int slotNumber,
    UpsertEquipmentSetSlotRequest request,
    CancellationToken cancellationToken);

  Task<bool> DeleteSlotAsync(long equipmentSetId, int slotNumber, CancellationToken cancellationToken);
}
