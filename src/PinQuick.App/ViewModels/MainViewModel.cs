using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using PinQuick.Core.Models;
using PinQuick.Core.Services;
using PinQuick.Windows;
using PinQuick.App.Services;

namespace PinQuick.App.ViewModels;

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
    public partial ObservableCollection<PinItemViewModel> RecentPins { get; set; } = new();

    [ObservableProperty]
    public partial PinItemViewModel? SelectedRecentPin { get; set; }

    [ObservableProperty]
    public partial ElementTheme RequestedTheme { get; set; } = ElementTheme.Default;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool CanReorder { get; set; }

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
            if (filter == PinFilter.WindowsSettings)
            {
                continue;
            }

            Filters.Add(new SidebarFilterItemViewModel(filter));
        }

        SelectedFilterItem = Filters.FirstOrDefault(f => f.Filter == CurrentFilter);
        SetTheme(AppSettings.Current.Theme);
    }

    partial void OnSearchQueryChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            RecordSearch(value.Trim());
        }

        ApplyFilter();
    }

    private void RecordSearch(string query)
    {
        var settings = AppSettings.Current;
        var history = settings.SearchHistory;
        history.RemoveAll(q => string.Equals(q, query, StringComparison.CurrentCultureIgnoreCase));
        history.Insert(0, query);

        const int maxEntries = 10;
        if (history.Count > maxEntries)
        {
            history.RemoveRange(maxEntries, history.Count - maxEntries);
        }

        settings.Save();
    }

    partial void OnCurrentFilterChanged(PinFilter value) => ApplyFilter();

    partial void OnSelectedFilterItemChanged(SidebarFilterItemViewModel? value)
    {
        if (value is null)
        {
            return;
        }

        SelectedCollection = null;
        if (value.Filter != CurrentFilter)
        {
            CurrentFilter = value.Filter;
        }
    }

    partial void OnSelectedCollectionChanged(Collection? value) => ApplyFilter();

    partial void OnSelectedRecentPinChanged(PinItemViewModel? value)
    {
        if (value is null)
        {
            return;
        }

        if (SelectedCollection is not null)
        {
            SelectedCollection = null;
        }

        if (CurrentFilter != PinFilter.All)
        {
            CurrentFilter = PinFilter.All;
        }

        SelectedPin = value;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            await LoadCollectionsAsync();
            var pins = await _pinManager.GetAllAsync();
            _allPins = pins.ToList();
            ApplyFilter();
            RefreshRecentPins();
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

        var sorted = CurrentFilter == PinFilter.Recent
            ? pinned.OrderByDescending(pin => pin.LastUsedAt ?? DateTime.MinValue)
            : pinned
                .OrderBy(pin => pin.SortOrder)
                .ThenBy(pin => pin.Title, StringComparer.CurrentCultureIgnoreCase);

        var canReorder = CurrentFilter == PinFilter.All
            && SelectedCollection is null
            && string.IsNullOrEmpty(query);
        if (CanReorder != canReorder)
        {
            CanReorder = canReorder;
        }

        Pins.Clear();
        foreach (var pin in sorted)
        {
            var item = new PinItemViewModel(pin);
            item.ToggleFavoriteRequested = OnToggleFavoriteRequested;
            Pins.Add(item);
        }

        StatusMessage = Pins.Count == 0 && _allPins.Count > 0 ? Loc.T("MsgNoPinsFound") : string.Empty;
    }

    private void RefreshRecentPins()
    {
        RecentPins.Clear();

        if (_allPins is null)
        {
            return;
        }

        const int maxRecent = 5;
        foreach (var pin in _allPins
                     .Where(p => p.LastUsedAt.HasValue)
                     .OrderByDescending(p => p.LastUsedAt)
                     .Take(maxRecent))
        {
            RecentPins.Add(new PinItemViewModel(pin));
        }
    }

    private async void OnToggleFavoriteRequested(PinItemViewModel item)
    {
        try
        {
            await PersistPinAsync(item.Pin);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            return;
        }

        ApplyFilter();
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
    private async Task RefreshAsync()
    {
        PinItemViewModel.ClearIconCache();
        await LoadAsync();
    }

    /// <summary>
    /// Mevcut filtre, arama ve koleksiyon koşullarını koruyarak kart listesini yeniden
    /// oluşturur. Kart boyutu gibi UI değerlerinin yansıması için kullanılır.
    /// </summary>
    public void RecreatePins() => ApplyFilter();

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
            StatusMessage = result.ErrorMessage is null
                ? Loc.T("MsgLaunchFailed")
                : $"{Loc.T("MsgLaunchFailed")}: {result.ErrorMessage}";
            return;
        }

        pin.LastUsedAt = DateTime.UtcNow;
        pin.UseCount++;
        await PersistPinAsync(pin);
        StatusMessage = string.Empty;
        RefreshRecentPins();

        if (CurrentFilter == PinFilter.Recent)
        {
            var selectedId = SelectedPin?.Id;
            ApplyFilter();
            SelectedPin = selectedId is long id
                ? Pins.FirstOrDefault(item => item.Id == id)
                : null;
        }
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
            Type = PinType.Folder,
            Target = GetFolderTarget(pin),
        };

        var result = _launcher.Start(locationPin);
        if (!result.Success)
        {
            StatusMessage = result.ErrorMessage is null
                ? Loc.T("MsgOpenLocationFailed")
                : $"{Loc.T("MsgOpenLocationFailed")}: {result.ErrorMessage}";
        }
    }

    /// <summary>
    /// Bir pinin hedefinin dosya sistemi üzerinde kullanılabilir olup olmadığını tip bilgisiyle birlikte değerlendirir.
    /// Yalnızca fiziksel hedefe sahip pin türleri için "konumu aç" anlamlıdır.
    /// </summary>
    public static bool CanOpenLocation(Pin pin)
        => pin.Type is PinType.Application or PinType.File or PinType.Folder
            or PinType.NetworkPath or PinType.Batch or PinType.PowerShell or PinType.SystemTool;

    /// <summary>
    /// Yönetici yetkisi gerektirebilecek pin türleri.
    /// </summary>
    public static bool CanRunAsAdmin(Pin pin)
        => pin.Type is PinType.Application or PinType.Command or PinType.PowerShell or PinType.Batch;

    private static string GetFolderTarget(Pin pin)
    {
        if (pin.Type is PinType.Folder or PinType.NetworkPath)
        {
            return pin.Target;
        }

        var path = PinQuick.Core.Security.PathValidation.ResolveEnvironmentVariables(pin.Target);
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

    public async Task<int> NextSortOrderAsync()
    {
        var pins = await _pinManager.GetAllAsync();
        return pins.Count == 0 ? 0 : pins.Max(pin => pin.SortOrder) + 1;
    }

    /// <summary>
    /// Kartların sürükle-bırak sonrası yeni sırasını veritabanına yazar.
    /// Yalnızca tüm pinler görünümünde (filtre, arama ve koleksiyon kapalıyken) anlamlıdır.
    /// </summary>
    public async Task PersistOrderAsync()
    {
        if (!CanReorder)
        {
            return;
        }

        var orderedItems = new List<(long Id, int SortOrder)>();
        for (var i = 0; i < Pins.Count; i++)
        {
            var pin = Pins[i].Pin;
            if (pin.Id == 0)
            {
                continue;
            }

            pin.SortOrder = i;
            orderedItems.Add((pin.Id, i));
        }

        if (orderedItems.Count > 0)
        {
            await _pinManager.ReorderAsync(orderedItems);
        }
    }

    public async Task AddOrUpdatePinAsync(Pin pin)
    {
        try
        {
            if (pin.Id == 0)
            {
                pin.SortOrder = await NextSortOrderAsync();
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
            StatusMessage = Loc.T("MsgAlreadyPinned");
            throw;
        }
    }

    /// <summary>
    /// Sürükle-bırak yoluyla gelen pinleri topluca ekler.
    /// </summary>
    public async Task AddDroppedPinsAsync(IReadOnlyList<Pin> pins)
    {
        if (pins is null || pins.Count == 0)
        {
            return;
        }

        var added = 0;
        var nextSortOrder = await NextSortOrderAsync();
        foreach (var pin in pins)
        {
            try
            {
                pin.SortOrder = nextSortOrder++;
                await _pinManager.AddAsync(pin);
                added++;
            }
            catch (DuplicatePinException)
            {
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        await LoadAsync();

        if (added == 0)
        {
            StatusMessage = Loc.T("MsgNothingToAdd");
        }
        else if (added == 1)
        {
            StatusMessage = Loc.T("MsgPinAddedOne");
        }
        else
        {
            StatusMessage = string.Format(Loc.T("MsgPinAddedMany"), added);
        }
    }

    public async Task AddCollectionAsync(string name, string? color = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var existing = await _collectionManager.GetAllAsync();
        if (existing.Any(collection =>
                string.Equals(collection.Name.Trim(), name.Trim(), StringComparison.CurrentCultureIgnoreCase)))
        {
            StatusMessage = Loc.T("MsgCollectionExists");
            return;
        }

        await _collectionManager.AddAsync(new Collection { Name = name.Trim(), Color = string.IsNullOrWhiteSpace(color) ? string.Empty : color.Trim() });
        await LoadCollectionsAsync();
    }

    public async Task UpdateCollectionAsync(Collection collection, string newName, string? color = null)
    {
        if (collection is null || string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        var existing = await _collectionManager.GetAllAsync();
        if (existing.Any(c =>
                c.Id != collection.Id
                && string.Equals(c.Name.Trim(), newName.Trim(), StringComparison.CurrentCultureIgnoreCase)))
        {
            StatusMessage = Loc.T("MsgCollectionExists");
            return;
        }

        var selectedId = SelectedCollection?.Id;
        collection.Name = newName.Trim();
        collection.Color = string.IsNullOrWhiteSpace(color) ? string.Empty : color.Trim();
        await _collectionManager.UpdateAsync(collection);
        await LoadCollectionsAsync();
        SelectedCollection = selectedId is long id
            ? Collections.FirstOrDefault(c => c.Id == id)
            : null;
        ApplyFilter();
    }

    public async Task AssignPinToCollectionAsync(Pin pin, long? collectionId)
    {
        if (pin is null)
        {
            return;
        }

        pin.CollectionId = collectionId;
        await _pinManager.UpdateAsync(pin);
        ApplyFilter();
    }

    public async Task AssignSelectedToCollectionAsync(IReadOnlyList<long> ids, long? collectionId)
    {
        if (ids is null || ids.Count == 0 || _allPins is null)
        {
            return;
        }

        var idSet = ids.ToHashSet();
        foreach (var pin in _allPins.Where(p => idSet.Contains(p.Id)))
        {
            pin.CollectionId = collectionId;
            await _pinManager.UpdateAsync(pin);
        }

        ApplyFilter();
    }

    public async Task DeleteSelectedAsync(IReadOnlyList<long> ids)
    {
        if (ids is null || ids.Count == 0)
        {
            return;
        }

        foreach (var id in ids)
        {
            await _pinManager.DeleteAsync(id);
            _allPins?.RemoveAll(pin => pin.Id == id);
        }

        SelectedPin = null;
        ApplyFilter();
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
        if (added == 0)
        {
            StatusMessage = Loc.T("MsgImportNothing");
        }
        else
        {
            StatusMessage = string.Format(Loc.T("MsgImportDone"), added);
        }

        return added;
    }

    public async Task ExportAsync(string filePath, PinFilter? filter = null, Collection? collection = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        var pins = await _pinManager.GetAllAsync();
        IEnumerable<Pin> selected = pins;

        if (filter is not null && filter != PinFilter.All)
        {
            selected = selected.Where(pin => PinFilterMapping.Matches(pin, filter.Value));
        }

        if (collection is not null && collection.Id != 0)
        {
            selected = selected.Where(pin => pin.CollectionId == collection.Id);
        }

        var list = selected.ToList();
        var json = PinExportService.Export(list);
        await File.WriteAllTextAsync(filePath, json);

        if (list.Count == 0)
        {
            StatusMessage = Loc.T("MsgExportNothing");
        }
        else if (filter is not null || collection is not null)
        {
            StatusMessage = string.Format(Loc.T("MsgExportFiltered"), list.Count);
        }
        else
        {
            StatusMessage = string.Format(Loc.T("MsgExportDone"), list.Count);
        }
    }
}