namespace PinQuick.Core.Models;

/// <summary>
/// Pinleri gruplamak için kullanılan kullanıcı koleksiyonu.
/// </summary>
public sealed class Collection
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}