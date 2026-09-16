using PinQuick.Core.Models;
using PinQuick.Core.Security;

namespace PinQuick.Tests;

public sealed class PinHealthTests
{
    [Fact]
    public void IsBroken_ApplicationProtocolUri_NotBroken()
    {
        var pin = new Pin
        {
            Title = "Steam Game",
            Type = PinType.Application,
            Target = "steam://rungameid/123456",
        };

        Assert.False(PinHealth.IsBroken(pin));
    }

    [Fact]
    public void IsBroken_ApplicationProtocolUri_CaseInsensitive()
    {
        var pin = new Pin
        {
            Title = "Steam Game",
            Type = PinType.Application,
            Target = "STEAM://rungameid/123456",
        };

        Assert.False(PinHealth.IsBroken(pin));
    }

    [Fact]
    public void IsBroken_ApplicationMissingFile_Broken()
    {
        var missing = Path.Combine(Path.GetTempPath(), $"pinquick-missing-{Guid.NewGuid():N}.exe");
        var pin = new Pin
        {
            Title = "Missing App",
            Type = PinType.Application,
            Target = missing,
        };

        Assert.True(PinHealth.IsBroken(pin));
    }

    [Fact]
    public void IsBroken_ApplicationExistingFile_NotBroken()
    {
        var existing = Path.Combine(Environment.SystemDirectory, "cmd.exe");
        var pin = new Pin
        {
            Title = "Cmd",
            Type = PinType.Application,
            Target = existing,
        };

        Assert.False(PinHealth.IsBroken(pin));
    }

    [Fact]
    public void IsBroken_WebsiteHttp_NotBroken()
    {
        var pin = new Pin
        {
            Title = "Site",
            Type = PinType.Website,
            Target = "https://example.com",
        };

        Assert.False(PinHealth.IsBroken(pin));
    }
}