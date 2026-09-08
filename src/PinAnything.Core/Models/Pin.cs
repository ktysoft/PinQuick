using PinAnything.Core.Models;

namespace PinAnything.Core.Models;

/// <summary>
/// Windows üzerinde sabitlenebilir bir öğeyi temsil eder.
/// </summary>
public sealed class Pin
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PinType Type { get; set; }
    public string Target { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public long? CollectionId { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int UseCount { get; set; }
    public int SortOrder { get; set; }
    public string Tags { get; set; } = string.Empty;
    public bool RunAsAdministrator { get; set; }
    public string OpenWith { get; set; } = string.Empty;
    public string CustomColor { get; set; } = string.Empty;
    public string CustomShortcut { get; set; } = string.Empty;
}
