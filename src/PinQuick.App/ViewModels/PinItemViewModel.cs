using System.Collections.Concurrent;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using PinQuick.Core.Models;
using PinQuick.App.Services;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace PinQuick.App.ViewModels;

public enum PinFilter
{
    All,
    Favorites,
    Applications,
    Folders,
    Files,
    Websites,
    Commands,
    Scripts,
    WindowsSettings,
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

    public string IconGlyph => IsGlyphCode(Pin.Icon) ? GlyphFromCode(Pin.Icon) : GetIconGlyph(Pin.Type);

    public BitmapImage? IconSource { get; private set; }

    public bool HasIconSource => IconSource is not null;

    public Action<PinItemViewModel>? ToggleFavoriteRequested { get; set; }

    [RelayCommand]
    private void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
        ToggleFavoriteRequested?.Invoke(this);
    }

    public PinItemViewModel(Pin pin)
    {
        Pin = pin ?? throw new ArgumentNullException(nameof(pin));
        _ = LoadIconAsync();
    }

    private static readonly ConcurrentDictionary<string, BitmapImage> IconCache = new();

    private async Task LoadIconAsync()
    {
        var iconValue = Pin.Icon?.Trim() ?? string.Empty;

        if (!string.IsNullOrEmpty(iconValue) && !IsGlyphCode(iconValue))
        {
            var image = await LoadIconImageAsync(iconValue, preferFolder: false);
            if (image is not null)
            {
                IconSource = image;
                OnPropertyChanged(nameof(IconSource));
                OnPropertyChanged(nameof(HasIconSource));
                return;
            }
        }

        if (string.IsNullOrEmpty(iconValue) && SupportsIcon(Pin.Type) && !string.IsNullOrWhiteSpace(Pin.Target))
        {
            var image = await LoadIconImageAsync(Pin.Target, preferFolder: Pin.Type is PinType.Folder);
            if (image is not null)
            {
                IconSource = image;
                OnPropertyChanged(nameof(IconSource));
                OnPropertyChanged(nameof(HasIconSource));
            }
        }
    }

    private static async Task<BitmapImage?> LoadIconImageAsync(string path, bool preferFolder)
    {
        if (IconCache.TryGetValue(path, out var cached))
        {
            return cached;
        }

        BitmapImage? bitmap;
        try
        {
            StorageItemThumbnail thumbnail = preferFolder
                ? await (await StorageFolder.GetFolderFromPathAsync(path)).GetThumbnailAsync(ThumbnailMode.SingleItem, 256)
                : await (await StorageFile.GetFileFromPathAsync(path)).GetThumbnailAsync(ThumbnailMode.SingleItem, 256);

            using (thumbnail)
            {
                bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(thumbnail);
            }
        }
        catch
        {
            return null;
        }

        IconCache[path] = bitmap;
        return bitmap;
    }

    public static void ClearIconCache() => IconCache.Clear();

    private static bool SupportsIcon(PinType type)
        => type is PinType.Application or PinType.File or PinType.Folder or PinType.Batch;

    public static bool IsGlyphCode(string? value)
        => !string.IsNullOrEmpty(value) && value.Length == 4 && value.All(Uri.IsHexDigit);

    public static string GlyphFromCode(string code)
        => char.ConvertFromUtf32(Convert.ToInt32(code, 16));

    public static string GetTypeDisplay(PinType type)
        => type switch
        {
            PinType.Application => Loc.T("TypeApplication"),
            PinType.File => Loc.T("TypeFile"),
            PinType.Folder => Loc.T("TypeFolder"),
            PinType.Website => Loc.T("TypeWebsite"),
            PinType.Url => Loc.T("TypeUrl"),
            PinType.Command => Loc.T("TypeCommand"),
            PinType.PowerShell => Loc.T("TypePowerShell"),
            PinType.Batch => Loc.T("TypeBatch"),
            PinType.WindowsSetting => Loc.T("TypeWindowsSetting"),
            PinType.SystemTool => Loc.T("TypeSystemTool"),
            PinType.NetworkPath => Loc.T("TypeNetworkPath"),
            PinType.Custom => Loc.T("TypeCustom"),
            _ => Loc.T("TypeCustom"),
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
            PinFilter.Favorites => pin.IsFavorite,
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
            PinFilter.All => Loc.T("FilterAll"),
            PinFilter.Favorites => Loc.T("FilterFavorites"),
            PinFilter.Applications => Loc.T("FilterApplications"),
            PinFilter.Folders => Loc.T("FilterFolders"),
            PinFilter.Files => Loc.T("FilterFiles"),
            PinFilter.Websites => Loc.T("FilterWebsites"),
            PinFilter.Commands => Loc.T("FilterCommands"),
            PinFilter.WindowsSettings => Loc.T("FilterSettings"),
            PinFilter.Scripts => Loc.T("FilterScripts"),
            _ => Loc.T("FilterAll"),
        };

    public static string GetIconGlyph(PinFilter filter)
        => filter switch
        {
            PinFilter.All => "\uE8A5",
            PinFilter.Favorites => "\uE734",
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