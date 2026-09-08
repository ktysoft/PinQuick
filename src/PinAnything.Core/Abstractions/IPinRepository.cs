using PinAnything.Core.Models;

namespace PinAnything.Core.Abstractions;

public interface IPinRepository
{
    Task<Pin?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Pin>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<long> AddAsync(Pin pin, CancellationToken cancellationToken = default);
    Task UpdateAsync(Pin pin, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<Pin?> FindDuplicateAsync(PinType type, string target, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Pin>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task ClearCollectionAsync(long collectionId, CancellationToken cancellationToken = default);
}