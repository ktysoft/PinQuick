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

        var results = await PinManager.SearchAsync("visual");

        var result = Assert.Single(results);
        Assert.Equal("Visual Studio Code", result.Title);
    }

    [Fact]
    public async Task SearchAsync_MatchesTarget()
    {
        await PinManager.AddAsync(CreatePin(title: "VS Code", target: @"C:\Program Files\Microsoft VS Code\Code.exe"));

        var results = await PinManager.SearchAsync("VS Code");

        Assert.Contains(results, p => p.Title == "VS Code");
    }

    [Fact]
    public async Task SearchAsync_MatchesTags()
    {
        var pin = CreatePin(title: "Sunucu Yönetimi");
        pin.Tags = "server network";
        await PinManager.AddAsync(pin);

        var results = await PinManager.SearchAsync("server");

        Assert.Contains(results, p => p.Title == "Sunucu Yönetimi");
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsAll()
    {
        await PinManager.AddAsync(CreatePin(title: "A", target: @"C:\A.exe"));
        await PinManager.AddAsync(CreatePin(title: "B", target: @"C:\B.exe"));

        var results = await PinManager.SearchAsync("");

        Assert.Equal(2, results.Count);
        Assert.Equal(2, (await PinManager.GetAllAsync()).Count);
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
    public async Task Favorites_AreSortedFirstInSearch()
    {
        await PinManager.AddAsync(CreatePin(title: "Alpha", target: @"C:\Alpha.exe"));
        var favorite = CreatePin(title: "Beta", target: @"C:\Beta.exe");
        favorite.IsFavorite = true;
        await PinManager.AddAsync(favorite);

        var results = await PinManager.SearchAsync("");

        Assert.Equal("Beta", results[0].Title);
    }
}