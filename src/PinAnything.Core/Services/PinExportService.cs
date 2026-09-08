using System.Text.Json;
using System.Text.Json.Serialization;
using PinAnything.Core.Models;

namespace PinAnything.Core.Services;

/// <summary>
/// Pinlerin JSON biçiminde dışa/içe aktarılması.
/// </summary>
public static class PinExportService
{
    private const int CurrentVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string Export(IEnumerable<Pin> pins)
    {
        ArgumentNullException.ThrowIfNull(pins);

        var export = new PinExportDocument
        {
            Version = CurrentVersion,
            Pins = pins.Select(Map).ToList(),
        };

        return JsonSerializer.Serialize(export, JsonOptions);
    }

    public static IReadOnlyList<PinExportItem> Parse(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        PinExportDocument? document;
        try
        {
            document = JsonSerializer.Deserialize<PinExportDocument>(json, JsonOptions);
        }
        catch (JsonException)
        {
            throw new FormatException("Import edilecek dosya geçersiz JSON içeriyor.");
        }

        if (document?.Pins is null || document.Version != CurrentVersion)
        {
            throw new FormatException("Import edilecek dosya beklenen Pin Anything formatında değil.");
        }

        return document.Pins;
    }

    public static Pin ToPin(PinExportItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (string.IsNullOrWhiteSpace(item.Title))
        {
            throw new FormatException("Başlığı boş olan pin atlandı.");
        }

        if (string.IsNullOrWhiteSpace(item.Target) && item.Type != nameof(PinType.Custom))
        {
            throw new FormatException($"'{item.Title}' pininin hedefi boş bırakılamaz.");
        }

        return new Pin
        {
            Title = item.Title.Trim(),
            Type = Enum.TryParse<PinType>(item.Type, ignoreCase: true, out var type) ? type : PinType.Custom,
            Target = item.Target?.Trim() ?? string.Empty,
            Description = item.Description?.Trim() ?? string.Empty,
            Tags = item.Tags?.Trim() ?? string.Empty,
            Arguments = item.Arguments?.Trim() ?? string.Empty,
            WorkingDirectory = item.WorkingDirectory?.Trim() ?? string.Empty,
            IsFavorite = item.IsFavorite,
            RunAsAdministrator = item.RunAsAdministrator,
            IsEnabled = true,
        };
    }

    private static PinExportItem Map(Pin pin)
    {
        return new PinExportItem
        {
            Title = pin.Title,
            Type = pin.Type.ToString(),
            Target = pin.Target,
            Description = pin.Description,
            Tags = pin.Tags,
            Arguments = pin.Arguments,
            WorkingDirectory = pin.WorkingDirectory,
            IsFavorite = pin.IsFavorite,
            RunAsAdministrator = pin.RunAsAdministrator,
        };
    }

    private sealed class PinExportDocument
    {
        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("pins")]
        public List<PinExportItem>? Pins { get; set; }
    }
}

public sealed class PinExportItem
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("target")]
    public string? Target { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("tags")]
    public string? Tags { get; set; }

    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }

    [JsonPropertyName("workingDirectory")]
    public string? WorkingDirectory { get; set; }

    [JsonPropertyName("favorite")]
    public bool IsFavorite { get; set; }

    [JsonPropertyName("runAsAdministrator")]
    public bool RunAsAdministrator { get; set; }
}