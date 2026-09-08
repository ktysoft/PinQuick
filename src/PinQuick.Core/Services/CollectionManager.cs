using PinQuick.Core.Abstractions;
using PinQuick.Core.Models;

namespace PinQuick.Core.Services;

public sealed class CollectionManager
{
    private readonly ICollectionRepository _repository;

    public CollectionManager(ICollectionRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<IReadOnlyList<Collection>> GetAllAsync(CancellationToken cancellationToken = default)
        => _repository.GetAllAsync(cancellationToken);

    public Task<Collection?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(id, cancellationToken);

    public async Task<long> AddAsync(Collection collection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (string.IsNullOrWhiteSpace(collection.Name))
        {
            throw new ArgumentException("Koleksiyon adı boş olamaz.", nameof(collection));
        }

        collection.CreatedAt = DateTime.UtcNow;
        collection.UpdatedAt = DateTime.UtcNow;

        return await _repository.AddAsync(collection, cancellationToken);
    }

    public async Task UpdateAsync(Collection collection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (collection.Id == 0)
        {
            throw new ArgumentException("Güncellenecek koleksiyonun Id değeri geçersiz.", nameof(collection));
        }

        collection.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(collection, cancellationToken);
    }

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(id, cancellationToken);
}