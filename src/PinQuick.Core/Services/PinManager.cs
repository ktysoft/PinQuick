using PinQuick.Core.Abstractions;
using PinQuick.Core.Models;

namespace PinQuick.Core.Services;

public sealed class PinManager
{
    private readonly IPinRepository _repository;

    public PinManager(IPinRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<IReadOnlyList<Pin>> GetAllAsync(CancellationToken cancellationToken = default)
        => _repository.GetAllAsync(cancellationToken);

    public Task<Pin?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(id, cancellationToken);

    public async Task<long> AddAsync(Pin pin, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pin);

        if (string.IsNullOrWhiteSpace(pin.Title))
        {
            throw new ArgumentException("Pin başlığı boş olamaz.", nameof(pin));
        }

        if (string.IsNullOrWhiteSpace(pin.Target) && pin.Type is not PinType.Custom)
        {
            throw new ArgumentException("Pin hedefi boş olamaz.", nameof(pin));
        }

        pin.CreatedAt = DateTime.UtcNow;
        pin.UpdatedAt = DateTime.UtcNow;

        var duplicate = await _repository.FindDuplicateAsync(pin.Type, pin.Target, cancellationToken);
        if (duplicate is not null)
        {
            throw new DuplicatePinException(duplicate);
        }

        return await _repository.AddAsync(pin, cancellationToken);
    }

    public async Task UpdateAsync(Pin pin, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pin);

        if (pin.Id == 0)
        {
            throw new ArgumentException("Güncellenecek pinin Id değeri geçersiz.", nameof(pin));
        }

        pin.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(pin, cancellationToken);
    }

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(id, cancellationToken);

    public Task ReorderAsync(IReadOnlyList<(long Id, int SortOrder)> orderedItems, CancellationToken cancellationToken = default)
        => _repository.ReorderAsync(orderedItems, cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => _repository.CountAsync(cancellationToken);

    public Task ClearCollectionAsync(long collectionId, CancellationToken cancellationToken = default)
        => _repository.ClearCollectionAsync(collectionId, cancellationToken);

    public async Task AddToCollectionAsync(long pinId, long collectionId, CancellationToken cancellationToken = default)
    {
        if (pinId <= 0 || collectionId <= 0)
        {
            throw new ArgumentException("Pin ve koleksiyon kimlikleri geçersiz.");
        }

        await _repository.AddToCollectionAsync(pinId, collectionId, cancellationToken);
    }

    public async Task RemoveFromCollectionAsync(long pinId, long collectionId, CancellationToken cancellationToken = default)
    {
        if (pinId <= 0 || collectionId <= 0)
        {
            throw new ArgumentException("Pin ve koleksiyon kimlikleri geçersiz.");
        }

        await _repository.RemoveFromCollectionAsync(pinId, collectionId, cancellationToken);
    }
}

public sealed class DuplicatePinException(Pin duplicate) : Exception("Bu öğe zaten pinlenmiş.")
{
    public Pin Duplicate { get; } = duplicate;
}