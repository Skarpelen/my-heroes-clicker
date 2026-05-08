using MyHeroesClicker.API.Contracts.Database;

namespace MyHeroesClicker.API.Core.Interfaces.Repositories;

public interface IAccountRepository
{
  Task<IReadOnlyCollection<AccountResponse>> GetAllAsync(CancellationToken cancellationToken);

  Task<AccountResponse?> GetByIdAsync(long id, CancellationToken cancellationToken);

  Task<long> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken);

  Task<bool> UpdateAsync(long id, UpdateAccountRequest request, CancellationToken cancellationToken);

  Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
}
