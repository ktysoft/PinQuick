using CommunityToolkit.Mvvm.ComponentModel;
using PinAnything.Core.Models;

namespace PinAnything_App.ViewModels;

public enum PinFilter
{
    All,
    Applications,
    Folders,
    Files,
    Websites,
    Commands,
    WindowsSettings,
    Scripts,
}

/// <summary>
/// UI için bir pinin sarmalayıcısı. Pin modeli değişmeden görünüm durumunu taşır.
/// </summary>
public sealed partial class PinItemViewModel : ObservableObject
{
    public Pin Pin { get; }

    public long Id => Pin.Id;
    public string Title => Pin.Title;
    public PinType Type => Pin.Type;
    public string TypeDisplay => GetTypeDisplay(Pin.Type);
    public string Target => Pin.Target;
    public string Description => Pin.Description;
    public string Tags => Pin.Tags;

    public bool IsFavorite
    {
        get => Pin.IsFavorite;
        set
        {
            if (Pin.IsFavorite == value)
            {
                return;
            }

            Pin.IsFavorite = value;
            OnPropertyChanged();
        }
    }

    public string IconGlyph => GetIconGlyph(Pin.Type);

    public PinItemViewModel(Pin pin)
    {
        Pin = pin ?? throw new ArgumentNullException(nameof(pin));
    }

    public static string GetTypeDisplay(PinType type)
        => type switch
        {
            PinType.Application => "Uygulama",
            PinType.File => "Dosya",
            PinType.Folder => "Klasör",
            PinType.Website => "Web Sitesi",
            PinType.Url => "URL",
            PinType.Command => "Komut",
            PinType.PowerShell => "PowerShell",
            PinType.Batch => "Batch",
            PinType.WindowsSetting => "Windows Ayarı",
            PinType.SystemTool => "Sistem Aracı",
            PinType.NetworkPath => "Ağ Yolu",
            PinType.Custom => "Özel",
            _ => "Pin",
        };

    public static string GetIconGlyph(PinType type)
        => type switch
        {
            PinType.Application => "\uE7F4",
            PinType.File => "\uE7C3",
            PinType.Folder => "\uE8B7",
            PinType.Website => "\uE774",
            PinType.Url => "\uE71B",
            PinType.Command => "\uE756",
            PinType.PowerShell => "\uE756",
            PinType.Batch => "\uE756",
            PinType.WindowsSetting => "\uE713",
            PinType.SystemTool => "\uE968",
            PinType.NetworkPath => "\uE968",
            PinType.Custom => "\uE8A7",
            _ => "\uE8A7",
        };
}

public static class PinFilterMapping
{
    public static bool Matches(Pin pin, PinFilter filter)
        => filter switch
        {
            PinFilter.All => true,
            PinFilter.Applications => pin.Type == PinType.Application,
            PinFilter.Folders => pin.Type == PinType.Folder,
            PinFilter.Files => pin.Type == PinType.File,
            PinFilter.Websites => pin.Type is PinType.Website or PinType.Url,
            PinFilter.Commands => pin.Type is PinType.Command or PinType.PowerShell or PinType.Batch,
            PinFilter.WindowsSettings => pin.Type is PinType.WindowsSetting or PinType.SystemTool,
            PinFilter.Scripts => pin.Type is PinType.PowerShell or PinType.Batch,
            _ => true,
        };

    public static string GetDisplayName(PinFilter filter)
        => filter switch
        {
            PinFilter.All => "Tüm Pinler",
            PinFilter.Applications => "Uygulamalar",
            PinFilter.Folders => "Klasörler",
            PinFilter.Files => "Dosyalar",
            PinFilter.Websites => "Web Siteleri",
            PinFilter.Commands => "Komutlar",
            PinFilter.WindowsSettings => "Ayarlar",
            PinFilter.Scripts => "Scripts",
            _ => "Tüm Pinler",
        };

    public static string GetIconGlyph(PinFilter filter)
        => filter switch
        {
            PinFilter.All => "\uE8A5",
            PinFilter.Applications => "\uE7F4",
            PinFilter.Folders => "\uE8B7",
            PinFilter.Files => "\uE7C3",
            PinFilter.Websites => "\uE774",
            PinFilter.Commands => "\uE756",
            PinFilter.WindowsSettings => "\uE713",
            PinFilter.Scripts => "\uE943",
            _ => "\uE8A5",
        };
}