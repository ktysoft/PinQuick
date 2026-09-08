using PinQuick.Core.Models;

namespace PinQuick.Tests;

public sealed class CollectionManagerTests : DatabaseTestBase
{
    public CollectionManagerTests()
    {
    }

    [Fact]
    public async Task AddAsync_AddsCollection()
    {
        var id = await CollectionManager.AddAsync(new Collection { Name = "Geliştirme" });

        var collection = await CollectionManager.GetByIdAsync(id);
        Assert.NotNull(collection);
        Assert.Equal("Geliştirme", collection!.Name);
    }

    [Fact]
    public async Task AddAsync_WithEmptyName_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => CollectionManager.AddAsync(new Collection { Name = "  " }));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesName()
    {
        var id = await CollectionManager.AddAsync(new Collection { Name = "Sistem" });

        var collection = await CollectionManager.GetByIdAsync(id);
        Assert.NotNull(collection);
        collection!.Name = "Sistem Yönetimi";

        await CollectionManager.UpdateAsync(collection);

        var updated = await CollectionManager.GetByIdAsync(id);
        Assert.Equal("Sistem Yönetimi", updated!.Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCollection()
    {
        var id = await CollectionManager.AddAsync(new Collection { Name = "Kişisel" });

        await CollectionManager.DeleteAsync(id);

        Assert.Null(await CollectionManager.GetByIdAsync(id));
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsNull()
    {
        Assert.Null(await CollectionManager.GetByIdAsync(9999));
    }

    [Fact]
    public async Task DeleteAsync_NonExisting_ThrowsKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => CollectionManager.DeleteAsync(9999));
    }

    [Fact]
    public async Task Pin_InCollection_CanBeCreated()
    {
        var collectionId = await CollectionManager.AddAsync(new Collection { Name = "Geliştirme" });

        var pin = CreatePin();
        pin.CollectionId = collectionId;
        var pinId = await PinManager.AddAsync(pin);

        var saved = await PinManager.GetByIdAsync(pinId);
        Assert.Equal(collectionId, saved!.CollectionId);
    }
}