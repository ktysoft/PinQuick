using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using PinAnything.Core.Models;
using PinAnything.Core.Services;
using PinAnything.Windows;

namespace PinAnything_App.ViewModels;

public sealed partial class SidebarFilterItemViewModel : ObservableObject
{
    public PinFilter Filter { get; }

    public string Name { get; }

    public string Glyph { get; }

    public SidebarFilterItemViewModel(PinFilter filter)
    {
        Filter = filter;
        Name = PinFilterMapping.GetDisplayName(filter);
        Glyph = PinFilterMapping.GetIconGlyph(filter);
    }
}

/// <summary>
/// Ana sayfanın ViewModel'i: pin listesi, filtreleme, arama ve pin işlemleri.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly PinManager _pinManager;
    private readonly CollectionManager _collectionManager;
    private readonly ProcessLauncher _launcher;
    private List<Pin>? _allPins;

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial PinFilter CurrentFilter { get; set; } = PinFilter.All;

    [ObservableProperty]
    public partial ObservableCollection<SidebarFilterItemViewModel> Filters { get; set; } = new();

    [ObservableProperty]
    public partial SidebarFilterItemViewModel? SelectedFilterItem { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<PinItemViewModel> Pins { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<Collection> Collections { get; set; } = new();

    [ObservableProperty]
    public partial Collection? SelectedCollection { get; set; }

    [ObservableProperty]
    public partial PinItemViewModel? SelectedPin { get; set; }

    [ObservableProperty]
    public partial ElementTheme RequestedTheme { get; set; } = ElementTheme.Default;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public MainViewModel(
        PinManager pinManager,
        CollectionManager collectionManager,
        ProcessLauncher launcher)
    {
        _pinManager = pinManager ?? throw new ArgumentNullException(nameof(pinManager));
        _collectionManager = collectionManager ?? throw new ArgumentNullException(nameof(collectionManager));
        _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));

        foreach (var filter in Enum.GetValues<PinFilter>())
        {
            Filters.Add(new SidebarFilterItemViewModel(filter));
        }

        SelectedFilterItem = Filters.FirstOrDefault(f => f.Filter == CurrentFilter);
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilter();

    partial void OnCurrentFilterChanged(PinFilter value) => ApplyFilter();

    partial void OnSelectedFilterItemChanged(SidebarFilterItemViewModel? value)
    {
        if (value is not null && value.Filter != CurrentFilter)
        {
            CurrentFilter = value.Filter;
        }
    }

    partial void OnSelectedCollectionChanged(Collection? value) => ApplyFilter();

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        try
        {
            await LoadCollectionsAsync();
            var pins = await _pinManager.GetAllAsync();
            _allPins = pins.ToList();
            ApplyFilter();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadCollectionsAsync()
    {
        var collections = await _collectionManager.GetAllAsync();
        Collections.Clear();
        foreach (var collection in collections)
        {
            Collections.Add(collection);
        }
    }

    private void ApplyFilter()
    {
        if (_allPins is null)
        {
            return;
        }

        var query = SearchQuery?.Trim() ?? string.Empty;
        var pinned = _allPins
            .Where(pin => PinFilterMapping.Matches(pin, CurrentFilter))
            .Where(pin => SelectedCollection is null || pin.CollectionId == SelectedCollection.Id);

        if (!string.IsNullOrEmpty(query))
        {
            pinned = pinned.Where(pin => MatchesQuery(pin, query));
        }

        var sorted = pinned
            .OrderByDescending(pin => pin.IsFavorite)
            .ThenBy(pin => pin.Title, StringComparer.CurrentCultureIgnoreCase);

        Pins.Clear();
        foreach (var pin in sorted)
        {
            Pins.Add(new PinItemViewModel(pin));
        }

        StatusMessage = Pins.Count == 0 && _allPins.Count > 0 ? "Pin bulunamadı." : string.Empty;
    }

    private static bool MatchesQuery(Pin pin, string query)
    {
        return pin.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase)
            || pin.Description.Contains(query, StringComparison.CurrentCultureIgnoreCase)
            || pin.Target.Contains(query, StringComparison.CurrentCultureIgnoreCase)
            || pin.Tags.Contains(query, StringComparison.CurrentCultureIgnoreCase)
            || PinItemViewModel.GetTypeDisplay(pin.Type).Contains(query, StringComparison.CurrentCultureIgnoreCase);
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedPin = null;
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync()
    {
        if (SelectedPin is null)
        {
            return;
        }

        SelectedPin.IsFavorite = !SelectedPin.IsFavorite;
        await PersistPinAsync(SelectedPin.Pin);
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedPin is null)
        {
            return;
        }

        var id = SelectedPin.Id;
        await _pinManager.DeleteAsync(id);
        _allPins?.RemoveAll(pin => pin.Id == id);
        SelectedPin = null;
        ApplyFilter();
    }

    [RelayCommand]
    private async Task LaunchAsync()
    {
        if (SelectedPin is null)
        {
            return;
        }

        await LaunchPinAsync(SelectedPin.Pin);
    }

    [RelayCommand]
    private async Task LaunchAsAdminAsync()
    {
        if (SelectedPin is null)
        {
            return;
        }

        var pin = SelectedPin.Pin;
        pin.RunAsAdministrator = true;
        await LaunchPinAsync(pin);
    }

    private async Task LaunchPinAsync(Pin pin)
    {
        var result = _launcher.Start(pin);
        if (!result.Success)
        {
            StatusMessage = result.ErrorMessage ?? "Pin başlatılamadı.";
            return;
        }

        pin.LastUsedAt = DateTime.UtcNow;
        pin.UseCount++;
        await PersistPinAsync(pin);
        StatusMessage = string.Empty;
    }

    private async Task PersistPinAsync(Pin pin)
    {
        if (pin.Id == 0)
        {
            return;
        }

        await _pinManager.UpdateAsync(pin);
    }

    [RelayCommand]
    private async Task OpenLocationAsync()
    {
        if (SelectedPin is null)
        {
            return;
        }

        var pin = SelectedPin.Pin;
        var locationPin = new Pin
        {
            Title = pin.Title,
            Type = pin.Type switch
            {
                PinType.Application or PinType.File =>
                    Path.HasExtension(pin.Target) ? PinType.Folder : PinType.Folder,
                PinType.Folder or PinType.NetworkPath => PinType.Folder,
                _ => PinType.Folder,
            },
            Target = GetFolderTarget(pin),
        };

        var result = _launcher.Start(locationPin);
        if (!result.Success)
        {
            StatusMessage = result.ErrorMessage ?? "Konum açılamadı.";
        }
    }

    private static string GetFolderTarget(Pin pin)
    {
        if (pin.Type is PinType.Folder or PinType.NetworkPath)
        {
            return pin.Target;
        }

        var path = PinAnything.Core.Security.PathValidation.ResolveEnvironmentVariables(pin.Target);
        var directory = Path.GetDirectoryName(path);
        return string.IsNullOrEmpty(directory) ? path : directory;
    }

    [RelayCommand]
    private void SetTheme(string value)
    {
        RequestedTheme = value switch
        {
            "Light" => ElementTheme.Light,
            "Dark" => ElementTheme.Dark,
            _ => ElementTheme.Default,
        };
    }

    public async Task AddOrUpdatePinAsync(Pin pin)
    {
        try
        {
            if (pin.Id == 0)
            {
                await _pinManager.AddAsync(pin);
            }
            else
            {
                await _pinManager.UpdateAsync(pin);
            }

            await LoadAsync();
        }
        catch (DuplicatePinException)
        {
            StatusMessage = "Bu öğe zaten pinlenmiş.";
            throw;
        }
    }

    public async Task AddCollectionAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var existing = await _collectionManager.GetAllAsync();
        if (existing.Any(collection =>
                string.Equals(collection.Name.Trim(), name.Trim(), StringComparison.CurrentCultureIgnoreCase)))
        {
            StatusMessage = "Bu isimde bir koleksiyon zaten var.";
            return;
        }

        await _collectionManager.AddAsync(new Collection { Name = name.Trim() });
        await LoadCollectionsAsync();
    }

    public async Task DeleteCollectionAsync(Collection collection)
    {
        if (collection is null || collection.Id == 0)
        {
            return;
        }

        await _collectionManager.DeleteAsync(collection.Id);
        await _pinManager.ClearCollectionAsync(collection.Id);

        if (SelectedCollection?.Id == collection.Id)
        {
            SelectedCollection = null;
        }

        await LoadAsync();
    }

    public async Task<int> ImportAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return 0;
        }

        var json = await File.ReadAllTextAsync(filePath);
        var items = PinExportService.Parse(json);

        var added = 0;
        foreach (var item in items)
        {
            try
            {
                await _pinManager.AddAsync(PinExportService.ToPin(item));
                added++;
            }
            catch (DuplicatePinException)
            {
            }
            catch (FormatException)
            {
            }
        }

        await LoadAsync();
        StatusMessage = added == 0 ? "Import edilecek yeni pin bulunamadı." : $"{added} pin içe aktarıldı.";
        return added;
    }

    public async Task ExportAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        var pins = await _pinManager.GetAllAsync();
        var json = PinExportService.Export(pins);
        await File.WriteAllTextAsync(filePath, json);
        StatusMessage = pins.Count == 0 ? "Dışa aktarılacak pin yok." : $"{pins.Count} pin dışa aktarıldı.";
    }
}