using PinQuick.Core.Models;
using PinQuick.Core.Services;
using PinQuick.Core.Security;

namespace PinQuick.Tests;

public sealed class PinExportServiceTests
{
    [Fact]
    public void Export_ProducesExpectedJsonShape()
    {
        var pin = new Pin
        {
            Title = "Visual Studio Code",
            Type = PinType.Application,
            Target = @"C:\Program Files\Microsoft VS Code\Code.exe",
            IsFavorite = true,
        };

        var json = PinExportService.Export(new[] { pin });

        Assert.Contains("\"version\": 1", json);
        Assert.Contains("\"title\": \"Visual Studio Code\"", json);
        Assert.Contains("\"type\": \"Application\"", json);
        Assert.Contains("\"favorite\": true", json);
    }

    [Fact]
    public void RoundTrip_PreservesCustomIcon()
    {
        var pin = new Pin
        {
            Title = "Steam",
            Type = PinType.Application,
            Target = @"C:\Steam\steam.exe",
            Icon = @"D:\icons\steam.ico",
        };

        var json = PinExportService.Export(new[] { pin });
        var items = PinExportService.Parse(json);

        var restored = PinExportService.ToPin(items[0]);
        Assert.Equal(@"D:\icons\steam.ico", restored.Icon);
    }

    [Fact]
    public void Parse_ValidJson_ReturnsItems()
    {
        var json = """
            {
              "version": 1,
              "pins": [
                {
                  "title": "VS Code",
                  "type": "Application",
                  "target": "C:\\Code.exe",
                  "favorite": true
                },
                {
                  "title": "GitHub",
                  "type": "Website",
                  "target": "https://github.com",
                  "favorite": false
                }
              ]
            }
            """;

        var items = PinExportService.Parse(json);

        Assert.Equal(2, items.Count);
        Assert.Equal("VS Code", items[0].Title);
        Assert.Equal("Website", items[1].Type);
    }

    [Fact]
    public void Parse_InvalidJson_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => PinExportService.Parse("{ not valid json"));
    }

    [Fact]
    public void Parse_WrongVersion_ThrowsFormatException()
    {
        var json = """{"version": 99, "pins": []}""";

        Assert.Throws<FormatException>(() => PinExportService.Parse(json));
    }

    [Fact]
    public void Parse_InvalidType_FallsBackToCustom()
    {
        var json = """
            {"version": 1, "pins": [{"title": "X", "type": "Bilinmeyen", "target": "y"}]}
            """;

        var items = PinExportService.Parse(json);

        var pin = PinExportService.ToPin(items[0]);
        Assert.Equal(PinType.Custom, pin.Type);
    }

    [Fact]
    public void ToPin_EmptyTitle_Throws()
    {
        var item = new PinExportItem { Title = " ", Type = "Application", Target = "x" };

        Assert.Throws<FormatException>(() => PinExportService.ToPin(item));
    }

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("not a url", false)]
    public void RoundTrip_Website_KeepsTarget(string target, bool expectedValid)
    {
        var pin = new Pin
        {
            Title = "Site",
            Type = PinType.Website,
            Target = target,
        };

        if (!expectedValid)
        {
            Assert.False(PathValidation.IsValidTarget(PinType.Website, target));
            return;
        }

        var json = PinExportService.Export(new[] { pin });
        var items = PinExportService.Parse(json);

        var restored = PinExportService.ToPin(items[0]);
        Assert.Equal("https://example.com", restored.Target);
        Assert.Equal(PinType.Website, restored.Type);
    }
}