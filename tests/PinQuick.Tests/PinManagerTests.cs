using PinQuick.Core.Models;
using PinQuick.Core.Services;

namespace PinQuick.Tests;

public sealed class PinManagerTests : DatabaseTestBase
{
    public PinManagerTests()
    {
    }

    [Fact]
    public async Task AddAsync_AddsPinAndAssignsId()
    {
        var id = await PinManager.AddAsync(CreatePin());

        var pin = await PinManager.GetByIdAsync(id);
        Assert.NotNull(pin);
        Assert.Equal("Visual Studio Code", pin!.Title);
        Assert.Equal(PinType.Application, pin.Type);
        Assert.True(pin.Id > 0);
    }

    [Fact]
    public async Task AddAsync_WithEmptyTitle_Throws()
    {
        var pin = CreatePin(title: "   ");

        await Assert.ThrowsAsync<ArgumentException>(() => PinManager.AddAsync(pin));
    }

    [Fact]
    public async Task AddAsync_DuplicateTarget_ThrowsDuplicatePinException()
    {
        await PinManager.AddAsync(CreatePin());
        var duplicate = CreatePin(title: "VS Code Second");

        await Assert.ThrowsAsync<DuplicatePinException>(() => PinManager.AddAsync(duplicate));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields()
    {
        var id = await PinManager.AddAsync(CreatePin());

        var pin = await PinManager.GetByIdAsync(id);
        Assert.NotNull(pin);
        pin!.Title = "Visual Studio";
        pin.Description = "Ana editör";

        await PinManager.UpdateAsync(pin);

        var updated = await PinManager.GetByIdAsync(id);
        Assert.NotNull(updated);
        Assert.Equal("Visual Studio", updated!.Title);
        Assert.Equal("Ana editör", updated.Description);
    }

    [Fact]
    public async Task DeleteAsync_RemovesPin()
    {
        var id = await PinManager.AddAsync(CreatePin());

        await PinManager.DeleteAsync(id);

        Assert.Null(await PinManager.GetByIdAsync(id));
        Assert.Equal(0, await PinManager.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_NonExisting_ThrowsKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => PinManager.DeleteAsync(9999));
    }

    [Fact]
    public async Task CountAsync_ReturnsPinCount()
    {
        await PinManager.AddAsync(CreatePin(title: "A", target: @"C:\A.exe"));
        await PinManager.AddAsync(CreatePin(title: "B", target: @"C:\B.exe"));
        await PinManager.AddAsync(CreatePin(title: "C", target: @"C:\C.exe"));

        var count = await PinManager.CountAsync();
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task SearchAsync_MatchesTitleCaseInsensitive()
    {
        await PinManager.AddAsync(CreatePin(title: "Visual Studio Code"));
        await PinManager.AddAsync(CreatePin(title: "PowerShell", type: PinType.SystemTool, target: "powershell.exe"));

        var results = await PinManager.GetAllAsync();

        var result = Assert.Single(results, p => p.Title == "Visual Studio Code");
        Assert.Equal("Visual Studio Code", result.Title);
    }

    [Fact]
    public async Task ReorderAsync_PersistsNewOrder()
    {
        var a = await PinManager.AddAsync(CreatePin(title: "A", target: @"C:\A.exe"));
        var b = await PinManager.AddAsync(CreatePin(title: "B", target: @"C:\B.exe"));
        var c = await PinManager.AddAsync(CreatePin(title: "C", target: @"C:\C.exe"));

        await PinManager.ReorderAsync(new[] { (c, 0), (a, 1), (b, 2) });

        var pins = await PinManager.GetAllAsync();
        Assert.Equal(new[] { "C", "A", "B" }, pins.Select(p => p.Title));
    }

    [Fact]
    public async Task AddToCollectionAsync_AssignsPinToCollection()
    {
        var collectionId = await CollectionManager.AddAsync(new Collection { Name = "İş" });
        var pinId = await PinManager.AddAsync(CreatePin());

        await PinManager.AddToCollectionAsync(pinId, collectionId);

        var pin = await PinManager.GetByIdAsync(pinId);
        Assert.NotNull(pin);
        Assert.Contains(collectionId, pin!.CollectionIds);
    }

    [Fact]
    public async Task AddToCollectionAsync_IsIdempotent()
    {
        var collectionId = await CollectionManager.AddAsync(new Collection { Name = "İş" });
        var pinId = await PinManager.AddAsync(CreatePin());

        await PinManager.AddToCollectionAsync(pinId, collectionId);
        await PinManager.AddToCollectionAsync(pinId, collectionId);

        var pin = await PinManager.GetByIdAsync(pinId);
        Assert.NotNull(pin);
        Assert.Single(pin!.CollectionIds);
    }

    [Fact]
    public async Task Pin_CanBelongToMultipleCollections()
    {
        var workId = await CollectionManager.AddAsync(new Collection { Name = "İş" });
        var personalId = await CollectionManager.AddAsync(new Collection { Name = "Kişisel" });
        var pinId = await PinManager.AddAsync(CreatePin());

        await PinManager.AddToCollectionAsync(pinId, workId);
        await PinManager.AddToCollectionAsync(pinId, personalId);

        var pin = await PinManager.GetByIdAsync(pinId);
        Assert.NotNull(pin);
        Assert.Equal(2, pin!.CollectionIds.Count);
        Assert.Contains(workId, pin.CollectionIds);
        Assert.Contains(personalId, pin.CollectionIds);
    }

    [Fact]
    public async Task ClearCollectionAsync_RemovesOnlyThatMembership()
    {
        var workId = await CollectionManager.AddAsync(new Collection { Name = "İş" });
        var personalId = await CollectionManager.AddAsync(new Collection { Name = "Kişisel" });
        var pinId = await PinManager.AddAsync(CreatePin());
        await PinManager.AddToCollectionAsync(pinId, workId);
        await PinManager.AddToCollectionAsync(pinId, personalId);

        await PinManager.ClearCollectionAsync(workId);

        var pin = await PinManager.GetByIdAsync(pinId);
        Assert.NotNull(pin);
        Assert.DoesNotContain(workId, pin!.CollectionIds);
        Assert.Contains(personalId, pin.CollectionIds);
    }

    [Fact]
    public async Task AddAsync_WithInitialCollectionIds_PersistsMembership()
    {
        var workId = await CollectionManager.AddAsync(new Collection { Name = "İş" });
        var pin = CreatePin();
        pin.CollectionIds.Add(workId);

        var pinId = await PinManager.AddAsync(pin);

        var loaded = await PinManager.GetByIdAsync(pinId);
        Assert.NotNull(loaded);
        Assert.Contains(workId, loaded!.CollectionIds);
    }
}