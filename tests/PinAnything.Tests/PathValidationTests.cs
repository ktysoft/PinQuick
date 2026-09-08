using PinAnything.Core.Models;
using PinAnything.Core.Security;

namespace PinAnything.Tests;

public sealed class PathValidationTests
{
    [Theory]
    [InlineData("https://github.com", true)]
    [InlineData("http://example.com", true)]
    [InlineData("ftp://example.com", false)]
    [InlineData("github.com", false)]
    [InlineData("not a url", false)]
    public void IsValidUrl_ValidatesTargets(string target, bool expected)
    {
        Assert.Equal(expected, PathValidation.IsValidUrl(target));
    }

    [Theory]
    [InlineData("ms-settings:network", true)]
    [InlineData("ms-settings:display", true)]
    [InlineData("https://example.com", false)]
    [InlineData("not-a-uri", false)]
    public void IsValidWindowsUri_ValidatesTargets(string target, bool expected)
    {
        Assert.Equal(expected, PathValidation.IsValidWindowsUri(target));
    }

    [Fact]
    public void IsValidTarget_Website_ValidUrl()
    {
        Assert.True(PathValidation.IsValidTarget(PinType.Website, "https://github.com"));
    }

    [Fact]
    public void IsValidTarget_Website_InvalidUrl()
    {
        Assert.False(PathValidation.IsValidTarget(PinType.Website, "github.com"));
    }

    [Fact]
    public void IsValidTarget_NetworkPath_RequiresUncPrefix()
    {
        Assert.True(PathValidation.IsValidTarget(PinType.NetworkPath, @"\\SERVER\Share"));
        Assert.False(PathValidation.IsValidTarget(PinType.NetworkPath, @"C:\Local\Path"));
    }

    [Fact]
    public void IsValidTarget_EmptyTarget_ReturnsFalse()
    {
        Assert.False(PathValidation.IsValidTarget(PinType.Folder, ""));
        Assert.False(PathValidation.IsValidTarget(PinType.Application, "   "));
    }

    [Fact]
    public void ResolveEnvironmentVariables_ExpandsKnownVariables()
    {
        var resolved = PathValidation.ResolveEnvironmentVariables("%USERPROFILE%\\Documents");

        Assert.DoesNotContain("%USERPROFILE%", resolved);
        Assert.EndsWith("\\Documents", resolved);
        Assert.Contains(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), resolved);
    }
}