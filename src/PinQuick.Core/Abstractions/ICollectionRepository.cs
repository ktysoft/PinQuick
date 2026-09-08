using PinQuick.Core.Models;

namespace PinQuick.Core.Abstractions;

public interface ICollectionRepository
{
    Task<Collection?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Collection>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<long> AddAsync(Collection collection, CancellationToken cancellationToken = default);
    Task UpdateAsync(Collection collection, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}