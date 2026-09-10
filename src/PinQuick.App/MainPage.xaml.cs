using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.Storage.Pickers;
using PinQuick.App.Dialogs;
using PinQuick.App.Services;
using PinQuick.App.ViewModels;
using PinQuick.Core.Models;
using PinQuick.Core.Services;

namespace PinQuick.App;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        InitializeComponent();
        RootGrid.AllowDrop = true;

        ViewModel = new MainViewModel(App.Services.PinManager, App.Services.CollectionManager, App.Services.ProcessLauncher);
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        ViewModel.Pins.CollectionChanged += (_, _) => UpdateEmptyState();
        ViewModel.RecentPins.CollectionChanged += (_, _) => UpdateRecentSectionVisibility();
        ViewModel.Collections.CollectionChanged += (_, _) => UpdateSelectionBar();
        Loaded += OnLoaded;
    }

    private void UpdateRecentSectionVisibility()
    {
        RecentSection.Visibility = ViewModel.RecentPins.Count > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadCommand.ExecuteAsync(null);
        UpdateEmptyState();
        UpdateRecentSectionVisibility();
        SearchBox.Focus(FocusState.Programmatic);
        UpdateThemeToggleGlyph();

        if (!AppSettings.Current.IsOnboardingCompleted)
        {
            AppSettings.Current.IsOnboardingCompleted = true;
            AppSettings.Current.Save();
            ShowTour();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedPin))
        {
            RefreshDetailPanel(ViewModel.SelectedPin);
        }
        else if (e.PropertyName == nameof(MainViewModel.RequestedTheme))
        {
            (App.Window as MainWindow)?.ApplyTheme(ViewModel.RequestedTheme);
        }
        else if (e.PropertyName == nameof(MainViewModel.StatusMessage))
        {
            UpdateStatusBar();
            UpdateEmptyState();
        }
    }

    private void UpdateEmptyState()
    {
        var showEmptyState = ViewModel.Pins.Count == 0 && string.IsNullOrEmpty(ViewModel.StatusMessage);
        EmptyStatePanel.Visibility = showEmptyState ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateStatusBar()
    {
        StatusBar.Visibility = string.IsNullOrEmpty(ViewModel.StatusMessage)
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private PinItemViewModel? _detailPin;

    private void RefreshDetailPanel(PinItemViewModel? pin)
    {
        if (_detailPin is not null)
        {
            _detailPin.PropertyChanged -= OnDetailPinPropertyChanged;
        }

        _detailPin = pin;

        if (pin is null)
        {
            DetailPanel.Visibility = Visibility.Collapsed;
            return;
        }

        pin.PropertyChanged += OnDetailPinPropertyChanged;

        DetailIcon.Glyph = pin.IconGlyph;
        var hasIcon = pin.HasIconSource;
        DetailIconThumb.Source = pin.IconSource;
        DetailIconThumb.Visibility = hasIcon ? Visibility.Visible : Visibility.Collapsed;
        DetailIcon.Visibility = hasIcon ? Visibility.Collapsed : Visibility.Visible;
        DetailTitle.Text = pin.Title;
        DetailTypeDisplay.Text = pin.TypeDisplay;
        DetailTarget.Text = pin.Target;

        if (!string.IsNullOrEmpty(pin.Description))
        {
            DetailDescription.Text = MarkdownFormatter.ToPlainText(pin.Description);
            DetailDescription.Visibility = Visibility.Visible;
        }
        else
        {
            DetailDescription.Visibility = Visibility.Collapsed;
        }

        if (!string.IsNullOrEmpty(pin.Tags))
        {
            DetailTags.Text = $"{Loc.T("FieldTags")}: {pin.Tags}";
            DetailTags.Visibility = Visibility.Visible;
        }
        else
        {
            DetailTags.Visibility = Visibility.Collapsed;
        }

        if (pin.LastUsedAt is null)
        {
            DetailLastUsed.Visibility = Visibility.Collapsed;
        }
        else
        {
            DetailLastUsed.Text = $"{Loc.T("DetailLastUsed")}: {pin.LastUsedAt.Value:g}";
            DetailLastUsed.Visibility = Visibility.Visible;
        }

        var canOpenLocation = MainViewModel.CanOpenLocation(pin.Pin);
        var canRunAsAdmin = MainViewModel.CanRunAsAdmin(pin.Pin);
        DetailOpenLocationButton.IsEnabled = canOpenLocation;
        DetailRunAsAdminButton.IsEnabled = canRunAsAdmin;

        DetailPanel.Visibility = Visibility.Visible;
    }

    private void OnDetailPinPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_detailPin is null)
        {
            return;
        }

        if (e.PropertyName is nameof(PinItemViewModel.IconSource) or nameof(PinItemViewModel.HasIconSource))
        {
            DetailIcon.Glyph = _detailPin.IconGlyph;
            var hasIcon = _detailPin.HasIconSource;
            DetailIconThumb.Source = _detailPin.IconSource;
            DetailIconThumb.Visibility = hasIcon ? Visibility.Visible : Visibility.Collapsed;
            DetailIcon.Visibility = hasIcon ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private async void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SettingsDialog
        {
            XamlRoot = RootGrid.XamlRoot,
        };

        var result = await dialog.ShowAsync();

        if (dialog.ShowTourRequested)
        {
            ShowTour();
            return;
        }

        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        var settings = AppSettings.Current;
        var languageChanged = !string.Equals(settings.Language, dialog.SelectedLanguage, StringComparison.Ordinal);

        ViewModel.SetThemeCommand.Execute(dialog.SelectedTheme);
        settings.Theme = dialog.SelectedTheme;
        settings.Language = dialog.SelectedLanguage;
        settings.MinimizeToTray = dialog.MinimizeToTrayEnabled;
        settings.GlobalHotkeyEnabled = dialog.GlobalHotkeyEnabled;
        settings.AutoBackupFrequency = dialog.SelectedAutoBackup;
        settings.Save();
        StartupManager.SetEnabled(dialog.StartupEnabled);
        (App.Window as MainWindow)?.ApplyNativeSettings();

        if (languageChanged)
        {
            var confirm = new ContentDialog
            {
                Title = Loc.T("RestartButton"),
                Content = Loc.T("RestartRequiredMessage"),
                PrimaryButtonText = Loc.T("RestartButton"),
                CloseButtonText = Loc.T("CancelButton"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = RootGrid.XamlRoot,
            };

            if (await confirm.ShowAsync() == ContentDialogResult.Primary)
            {
                App.Restart();
            }
        }
    }

    private void RootGrid_DragOver(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }
        else
        {
            e.AcceptedOperation = DataPackageOperation.None;
        }
    }

    private async void RootGrid_Drop(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            return;
        }

        var items = await e.DataView.GetStorageItemsAsync();
        if (items is null || items.Count == 0)
        {
            return;
        }

        var pins = new List<Pin>();
        foreach (var item in items)
        {
            switch (item)
            {
                case IStorageFile file:
                {
                    var extension = Path.GetExtension(file.Path).ToLowerInvariant();
                    var type = extension switch
                    {
                        ".exe" or ".lnk" => PinType.Application,
                        ".bat" or ".cmd" => PinType.Batch,
                        ".ps1" => PinType.PowerShell,
                        _ => PinType.File,
                    };
                    pins.Add(new Pin
                    {
                        Title = Path.GetFileNameWithoutExtension(file.Path),
                        Type = type,
                        Target = file.Path,
                    });
                    break;
                }
                case IStorageFolder folder:
                    pins.Add(new Pin
                    {
                        Title = folder.Name,
                        Type = PinType.Folder,
                        Target = folder.Path,
                    });
                    break;
            }
        }

        if (pins.Count > 0)
        {
            await ViewModel.AddDroppedPinsAsync(pins);
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.RefreshCommand.ExecuteAsync(null);
    }

    private void RefreshAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        _ = ViewModel.RefreshCommand.ExecuteAsync(null);
    }

    private void CardSizeFlyout_Opened(object? sender, object e)
    {
        var size = AppSettings.Current.CardSize;
        CardSizeSmallRadio.IsChecked = size == "Small";
        CardSizeMediumRadio.IsChecked = size == "Medium";
        CardSizeLargeRadio.IsChecked = size == "Large";
    }

    private void CardSizeRadio_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton { Tag: string size } || size == AppSettings.Current.CardSize)
        {
            return;
        }

        AppSettings.Current.CardSize = size;
        AppSettings.Current.Save();
        ViewModel.RecreatePins();
    }

    private async void NewPinButton_Click(object sender, RoutedEventArgs e)
    {
        await OpenPinDialogAsync(null);
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        await OpenPinDialogAsync(ViewModel.SelectedPin?.Pin);
    }

    private async Task OpenPinDialogAsync(Pin? existing)
    {
        var dialog = new PinDialog(existing, ViewModel.Collections.ToList())
        {
            XamlRoot = RootGrid.XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        try
        {
            await ViewModel.AddOrUpdatePinAsync(dialog.Result);
        }
        catch (DuplicatePinException)
        {
        }
    }

    private async void NewCollectionButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CollectionDialog { XamlRoot = RootGrid.XamlRoot };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.ResultName is not null)
        {
            await ViewModel.AddCollectionAsync(dialog.ResultName, dialog.ResultColor);
        }
    }

    private async void DeleteCollectionButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCollection is null)
        {
            return;
        }

        var confirm = new ContentDialog
        {
            Title = Loc.T("DeleteCollectionTitle"),
            Content = $"{string.Format(Loc.T("DeleteCollectionPrompt"), ViewModel.SelectedCollection.Name)} İçindeki pinler korunur.",
            PrimaryButtonText = Loc.T("SilButton"),
            CloseButtonText = Loc.T("CancelButton"),
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = RootGrid.XamlRoot,
        };

        if (await confirm.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteCollectionAsync(ViewModel.SelectedCollection);
        }
    }

    private async void EditCollectionButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCollection is null)
        {
            return;
        }

        var dialog = new CollectionDialog(ViewModel.SelectedCollection.Name, ViewModel.SelectedCollection.Color)
        {
            XamlRoot = RootGrid.XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.ResultName is not null)
        {
            await ViewModel.UpdateCollectionAsync(ViewModel.SelectedCollection, dialog.ResultName, dialog.ResultColor);
        }
    }

    private void LaunchButton_Click(object sender, RoutedEventArgs e)
        => ViewModel.LaunchCommand.Execute(null);

    private void LaunchAdminButton_Click(object sender, RoutedEventArgs e)
        => ViewModel.LaunchAsAdminCommand.Execute(null);

    private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        => ViewModel.ToggleFavoriteCommand.Execute(null);

    private void OpenLocationButton_Click(object sender, RoutedEventArgs e)
        => ViewModel.OpenLocationCommand.Execute(null);

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (PinGrid.SelectedItems.Count > 1)
        {
            await DeleteSelectedPinsAsync();
            return;
        }

        if (ViewModel.SelectedPin is null)
        {
            return;
        }

        var confirm = new ContentDialog
        {
            Title = Loc.T("DeletePinTitle"),
            Content = string.Format(Loc.T("DeletePinPrompt"), ViewModel.SelectedPin.Title),
            PrimaryButtonText = Loc.T("SilButton"),
            CloseButtonText = Loc.T("CancelButton"),
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = RootGrid.XamlRoot,
        };

        if (await confirm.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteSelectedCommand.ExecuteAsync(null);
            RefreshDetailPanel(null);
        }
    }

    private void PinGrid_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not PinItemViewModel item)
        {
            return;
        }

        if (!IsMultiSelectModifierPressed())
        {
            ViewModel.SelectedPin = item;
        }
    }

    private static bool IsMultiSelectModifierPressed()
    {
        var ctrl = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control);
        var shift = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
        return ctrl.HasFlag(CoreVirtualKeyStates.Down)
            || shift.HasFlag(CoreVirtualKeyStates.Down);
    }

    private void FiltersList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not SidebarFilterItemViewModel item)
        {
            return;
        }

        ViewModel.SelectedCollection = null;
        ViewModel.SelectedFilterItem = item;
    }

    private async void PinGrid_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
    {
        if (args.DropResult == DataPackageOperation.Move)
        {
            await ViewModel.PersistOrderAsync();
        }
    }

    private void PinGrid_DragItemsStarting(object sender, DragItemsStartingEventArgs args)
    {
        var ids = args.Items
            .OfType<PinItemViewModel>()
            .Where(p => p.Id != 0)
            .Select(p => p.Id.ToString())
            .ToList();

        if (ids.Count > 0)
        {
            args.Data.SetText(string.Join(",", ids));
        }
        else
        {
            args.Cancel = true;
        }
    }

    private void PinGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateSelectionBar();
    }

    private void UpdateSelectionBar()
    {
        var count = PinGrid.SelectedItems.Count;
        if (count > 1)
        {
            SelectionCountText.Text = string.Format(Loc.T("SelBarCount"), count);
            SelectionAddButton.IsEnabled = ViewModel.Collections.Count > 0;
            SelectionBar.Visibility = Visibility.Visible;
        }
        else
        {
            SelectionBar.Visibility = Visibility.Collapsed;
        }
    }

    private void SelectionClearButton_Click(object sender, RoutedEventArgs e)
    {
        PinGrid.SelectedItem = null;
    }

    private async void SelectionDeleteButton_Click(object sender, RoutedEventArgs e)
        => await DeleteSelectedPinsAsync();

    private async void SelectionAddButton_Click(object sender, RoutedEventArgs e)
    {
        var ids = PinGrid.SelectedItems.OfType<PinItemViewModel>().Select(p => p.Id).ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var flyout = new MenuFlyout();
        foreach (var collection in ViewModel.Collections)
        {
            var item = new MenuFlyoutItem { Text = collection.Name };
            var collectionId = collection.Id;
            item.Click += async (_, _) => await ViewModel.AssignSelectedToCollectionAsync(ids, collectionId);
            flyout.Items.Add(item);
        }

        if (flyout.Items.Count == 0)
        {
            return;
        }

        if (sender is FrameworkElement element)
        {
            flyout.ShowAt(element);
        }
    }

    private void BuildMultiSelectMenu(MenuFlyout menu)
    {
        var count = PinGrid.SelectedItems.Count;

        if (ViewModel.Collections.Count > 0)
        {
            var addToCollection = new MenuFlyoutSubItem
            {
                Text = Loc.T("ContextAddToCollection"),
                Icon = new FontIcon { Glyph = "\uE8B7" },
            };
            foreach (var collection in ViewModel.Collections)
            {
                var collectionItem = new MenuFlyoutItem { Text = collection.Name };
                collectionItem.Click += async (_, _) => await ViewModel.AssignSelectedToCollectionAsync(
                    PinGrid.SelectedItems.OfType<PinItemViewModel>().Select(p => p.Id).ToList(), collection.Id);
                addToCollection.Items.Add(collectionItem);
            }
            menu.Items.Add(addToCollection);
        }

        var delete = new MenuFlyoutItem
        {
            Text = $"{Loc.T("DetailDelete")} ({count})",
            Icon = new FontIcon { Glyph = "\uE74D" },
        };
        delete.Click += async (_, _) => await DeleteSelectedPinsAsync();
        menu.Items.Add(delete);
    }

    private async Task DeleteSelectedPinsAsync()
    {
        var count = PinGrid.SelectedItems.Count;
        if (count == 0)
        {
            return;
        }

        var confirm = new ContentDialog
        {
            Title = Loc.T("DeletePinTitle"),
            Content = string.Format(Loc.T("DeleteSelectedPrompt"), count),
            PrimaryButtonText = Loc.T("SilButton"),
            CloseButtonText = Loc.T("CancelButton"),
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = RootGrid.XamlRoot,
        };

        if (await confirm.ShowAsync() != ContentDialogResult.Primary)
        {
            return;
        }

        var ids = PinGrid.SelectedItems.OfType<PinItemViewModel>().Select(p => p.Id).ToList();
        await ViewModel.DeleteSelectedAsync(ids);
        RefreshDetailPanel(null);
    }

    private void PinCard_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (sender is not FrameworkElement element
            || element.DataContext is not PinItemViewModel item)
        {
            return;
        }

        if (PinGrid.SelectedItems.Count <= 1)
        {
            ViewModel.SelectedPin = item;
        }

        var menu = new MenuFlyout();

        if (PinGrid.SelectedItems.Count > 1)
        {
            BuildMultiSelectMenu(menu);
            menu.ShowAt(element, e.GetPosition(element));
            return;
        }

        var open = new MenuFlyoutItem { Text = Loc.T("DetailOpen"), Icon = new FontIcon { Glyph = "\uE768" } };
        open.Click += (_, _) => _ = ViewModel.LaunchCommand.ExecuteAsync(null);
        menu.Items.Add(open);

        if (item.Type is PinType.Application)
        {
            var admin = new MenuFlyoutItem { Text = Loc.T("DetailRunAsAdmin"), Icon = new FontIcon { Glyph = "\uE882" } };
            admin.Click += (_, _) => ViewModel.LaunchAsAdminCommand.Execute(null);
            menu.Items.Add(admin);
        }

        var location = new MenuFlyoutItem { Text = Loc.T("DetailOpenLocation"), Icon = new FontIcon { Glyph = "\uE81D" } };
        location.Click += (_, _) => ViewModel.OpenLocationCommand.Execute(null);
        menu.Items.Add(location);

        var favorite = new MenuFlyoutItem
        {
            Text = item.IsFavorite ? Loc.T("ContextRemoveFavorite") : Loc.T("ContextAddFavorite"),
            Icon = new FontIcon { Glyph = item.IsFavorite ? "\uE735" : "\uE734" },
        };
        favorite.Click += async (_, _) => await ViewModel.ToggleFavoriteCommand.ExecuteAsync(null);
        menu.Items.Add(favorite);

        if (ViewModel.Collections.Count > 0)
        {
            var addToCollection = new MenuFlyoutSubItem
            {
                Text = Loc.T("ContextAddToCollection"),
                Icon = new FontIcon { Glyph = "\uE8B7" },
            };
            foreach (var collection in ViewModel.Collections)
            {
                var collectionItem = new MenuFlyoutItem { Text = collection.Name };
                collectionItem.Click += async (_, _) => await ViewModel.AssignPinToCollectionAsync(item.Pin, collection.Id);
                addToCollection.Items.Add(collectionItem);
            }

            menu.Items.Add(addToCollection);
        }

        var edit = new MenuFlyoutItem { Text = Loc.T("DetailEdit"), Icon = new FontIcon { Glyph = "\uE70F" } };
        edit.Click += (_, _) => _ = OpenPinDialogAsync(item.Pin);
        menu.Items.Add(edit);

        var delete = new MenuFlyoutItem { Text = Loc.T("DetailDelete"), Icon = new FontIcon { Glyph = "\uE74D" } };
        delete.Click += (_, _) => DeleteButton_Click(this, new RoutedEventArgs());
        menu.Items.Add(delete);

        menu.ShowAt(element, e.GetPosition(element));
    }

    private void PinGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if ((e.OriginalSource as FrameworkElement)?.DataContext is PinItemViewModel item)
        {
            ViewModel.SelectedPin = item;
            ViewModel.LaunchCommand.Execute(null);
        }
    }

    private bool _isCollectionsExpanded = true;

    private void CollectionsToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _isCollectionsExpanded = !_isCollectionsExpanded;
        CollectionsList.Visibility = _isCollectionsExpanded ? Visibility.Visible : Visibility.Collapsed;
        CollectionsRow.Height = _isCollectionsExpanded
            ? new GridLength(1, GridUnitType.Star)
            : GridLength.Auto;
        CollectionsChevron.Glyph = _isCollectionsExpanded ? "\uE70D" : "\uE76C";
    }

    private async void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var content = new StackPanel { Spacing = 8, MinWidth = 320 };

        content.Children.Add(new TextBlock
        {
            Text = AppInfo.Title,
            FontSize = 18,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
        });
        content.Children.Add(new TextBlock
        {
            Text = Loc.T("AboutDescription"),
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.8,
        });
        content.Children.Add(new TextBlock { Text = string.Format(Loc.T("AboutVersion"), AppInfo.Version), Margin = new Thickness(0, 8, 0, 0) });
        content.Children.Add(new TextBlock { Text = string.Format(Loc.T("AboutDeveloper"), AppInfo.Developer) });
        content.Children.Add(CreateLinkText(Loc.T("AboutWebsite"), $"https://{AppInfo.Website}"));
        content.Children.Add(CreateLinkText(Loc.T("AboutEmail"), $"mailto:{AppInfo.Email}"));
        content.Children.Add(new TextBlock { Text = string.Format(Loc.T("AboutLicense"), AppInfo.License) });

        var dialog = new ContentDialog
        {
            Title = string.Format(Loc.T("AboutTitle"), AppInfo.Title),
            Content = content,
            CloseButtonText = Loc.T("CloseButton"),
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = RootGrid.XamlRoot,
        };

        await dialog.ShowAsync();
    }

    private static StackPanel CreateLinkText(string labelFormat, string uri)
    {
        var start = labelFormat.IndexOf("{0}", StringComparison.Ordinal);
        var prefix = start > 0 ? labelFormat[..start] : string.Empty;
        var suffix = start >= 0 ? labelFormat[(start + 3)..] : string.Empty;
        var link = uri;
        var display = link.Contains("mailto:", StringComparison.OrdinalIgnoreCase)
            ? link[7..]
            : link.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase);

        var content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 0,
            VerticalAlignment = VerticalAlignment.Center,
        };
        if (!string.IsNullOrEmpty(prefix))
        {
            content.Children.Add(new TextBlock { Text = prefix, VerticalAlignment = VerticalAlignment.Center });
        }

        var hyperlink = new HyperlinkButton
        {
            Content = display,
            Padding = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        hyperlink.Click += (_, _) => LaunchUri(link);
        content.Children.Add(hyperlink);

        if (!string.IsNullOrEmpty(suffix))
        {
            content.Children.Add(new TextBlock { Text = suffix, VerticalAlignment = VerticalAlignment.Center });
        }

        return content;
    }

    private static async void LaunchUri(string uri)
    {
        try
        {
            await global::Windows.System.Launcher.LaunchUriAsync(new Uri(uri));
        }
        catch (Exception)
        {
        }
    }

    private async void ExportMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var filter = await ShowExportFilterDialogAsync();
        if (filter is null)
        {
            return;
        }

        var picker = new FileSavePicker(App.Window.AppWindow.Id)
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = "pinquick-pins",
        };
        picker.FileTypeChoices.Add("JSON", new List<string> { ".json" });

        var file = await picker.PickSaveFileAsync();
        if (file is not null)
        {
            await ViewModel.ExportAsync(file.Path, filter);
        }
    }

    private async Task<PinFilter?> ShowExportFilterDialogAsync()
    {
        var dialog = new ContentDialog
        {
            Title = Loc.T("ExportFilterTitle"),
            Content = new RadioButtons
            {
                ItemsSource = new List<string> { Loc.T("ExportFilterAll"), Loc.T("ExportFilterCurrent") },
                SelectedIndex = 0,
            },
            PrimaryButtonText = Loc.T("MenuExport"),
            CloseButtonText = Loc.T("CancelButton"),
            XamlRoot = RootGrid.XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return null;
        }

        return dialog.Content is RadioButtons { SelectedIndex: 1 } ? ViewModel.CurrentFilter : PinFilter.All;
    }

    private async void ImportMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker(App.Window.AppWindow.Id)
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        };
        picker.FileTypeFilter.Add(".json");

        var file = await picker.PickSingleFileAsync();
        if (file is not null)
        {
            try
            {
                await ViewModel.ImportAsync(file.Path);
            }
            catch (FormatException ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        }
    }

    private async void BackupMenuItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var path = BackupService.CreateBackup();
            var dialog = new ContentDialog
            {
                Title = Loc.T("MsgBackupCreated"),
                Content = path,
                CloseButtonText = Loc.T("CloseButton"),
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = RootGrid.XamlRoot,
            };
            await dialog.ShowAsync();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            await ShowErrorAsync(Loc.T("MsgBackupFailed"));
        }
    }

    private async void RestoreMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker(App.Window.AppWindow.Id)
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        };
        picker.FileTypeFilter.Add(".zip");

        var file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            return;
        }

        var confirm = new ContentDialog
        {
            Title = Loc.T("MenuRestore"),
            Content = Loc.T("RestoreConfirmMessage"),
            PrimaryButtonText = Loc.T("RestoreButton"),
            CloseButtonText = Loc.T("CancelButton"),
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = RootGrid.XamlRoot,
        };

        if (await confirm.ShowAsync() != ContentDialogResult.Primary)
        {
            return;
        }

        try
        {
            BackupService.RestoreBackup(file.Path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.IO.InvalidDataException)
        {
            await ShowErrorAsync(Loc.T("MsgRestoreFailed"));
            return;
        }

        App.Restart();
    }

    private void ShowTourMenuItem_Click(object sender, RoutedEventArgs e)
        => ShowTour();

    private async Task ShowErrorAsync(string message)
    {
        var dialog = new ContentDialog
        {
            Title = Loc.T("ErrorTitle"),
            Content = message,
            CloseButtonText = Loc.T("CloseButton"),
            XamlRoot = RootGrid.XamlRoot,
        };
        await dialog.ShowAsync();
    }

    private void SearchAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        SearchBox.Focus(FocusState.Programmatic);
        SearchBox.SelectAll();
        args.Handled = true;
    }

    private void ClearSearch_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.SearchQuery = string.Empty;
        args.Handled = true;
    }

    private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ViewModel.SearchQuery))
        {
            return;
        }

        var history = AppSettings.Current.SearchHistory;
        if (history.Count == 0)
        {
            return;
        }

        var panel = new StackPanel { Spacing = 2, MinWidth = 320 };
        var header = new TextBlock
        {
            Text = Loc.T("SearchHistoryLabel"),
            FontSize = 12,
            Opacity = 0.6,
            Margin = new Thickness(4, 2, 4, 4),
        };
        panel.Children.Add(header);

        foreach (var query in history)
        {
            var item = new Button
            {
                Content = query,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
            };
            item.Click += (_, _) =>
            {
                ViewModel.SearchQuery = query;
                SearchBox.Text = query;
                FlyoutBase.ShowAttachedFlyout(SearchBox);
                FlyoutBase.GetAttachedFlyout(SearchBox)?.Hide();
            };
            panel.Children.Add(item);
        }

        var clear = new Button
        {
            Content = Loc.T("SearchHistoryClear"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 6, 0, 0),
        };
        clear.Click += (_, _) =>
        {
            AppSettings.Current.SearchHistory.Clear();
            AppSettings.Current.Save();
            FlyoutBase.GetAttachedFlyout(SearchBox)?.Hide();
        };
        panel.Children.Add(clear);

        FlyoutBase.SetAttachedFlyout(SearchBox, new Flyout { Content = panel });
        FlyoutBase.ShowAttachedFlyout(SearchBox);
    }

    private void EscapeAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (TourOverlay.Visibility == Visibility.Visible)
        {
            CloseTour();
            args.Handled = true;
        }
        else if (PinGrid.SelectedItems.Count > 1)
        {
            PinGrid.SelectedItem = null;
            args.Handled = true;
        }
    }

    private void NewPinAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        _ = OpenPinDialogAsync(null);
        args.Handled = true;
    }

    private void EditAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (ViewModel.SelectedPin is not null)
        {
            _ = OpenPinDialogAsync(ViewModel.SelectedPin.Pin);
        }

        args.Handled = true;
    }

    private void DeleteAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (ViewModel.SelectedPin is not null)
        {
            DeleteButton_Click(this, new RoutedEventArgs());
        }

        args.Handled = true;
    }

    private void ExportAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ExportMenuItem_Click(this, new RoutedEventArgs());
        args.Handled = true;
    }

    private void ImportAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ImportMenuItem_Click(this, new RoutedEventArgs());
        args.Handled = true;
    }

    private int _tourIndex = -1;
    private FrameworkElement? _tourTarget;
    private (FrameworkElement Target, string TitleKey, string SubtitleKey)[] _tourSteps = null!;

    private void EnsureTourSteps()
    {
        if (_tourSteps is not null)
        {
            return;
        }

        _tourSteps =
        [
            (SearchBox, "TourStep1Title", "TourStep1Subtitle"),
            (NewPinButton, "TourStep2Title", "TourStep2Subtitle"),
            (FiltersList, "TourStep3Title", "TourStep3Subtitle"),
            (CollectionsList, "TourStep4Title", "TourStep4Subtitle"),
            (PinGrid, "TourStep5Title", "TourStep5Subtitle"),
            (SettingsButton, "TourStep6Title", "TourStep6Subtitle"),
            (MenuButton, "TourStep7Title", "TourStep7Subtitle"),
        ];
    }

    private void ShowTour()
    {
        EnsureTourSteps();

        if (TourOverlay.Visibility == Visibility.Visible)
        {
            return;
        }

        _tourIndex = 0;
        TourOverlay.Visibility = Visibility.Visible;
        ShowTourStep();
    }

    private void ShowTourStep()
    {
        var (target, titleKey, subtitleKey) = _tourSteps[_tourIndex];

        if (ReferenceEquals(target, CollectionsList) && CollectionsList.Visibility != Visibility.Visible)
        {
            _isCollectionsExpanded = true;
            CollectionsList.Visibility = Visibility.Visible;
            CollectionsRow.Height = new GridLength(1, GridUnitType.Star);
            CollectionsChevron.Glyph = "\uE70D";
        }

        TourStepTitle.Text = Loc.T(titleKey);
        TourStepSubtitle.Text = Loc.T(subtitleKey);
        TourStepNumber.Text = string.Format(Loc.T("TourProgress"), _tourIndex + 1, _tourSteps.Length);
        TourNextButton.Content = _tourIndex == _tourSteps.Length - 1
            ? Loc.T("TourFinish")
            : Loc.T("TourNext");

        _tourTarget = target;
        PositionTourHighlight();
    }

    private void PositionTourHighlight()
    {
        if (_tourTarget is null || TourOverlay.Visibility != Visibility.Visible)
        {
            return;
        }

        const double padding = 8;

        _tourTarget.UpdateLayout();
        TourOverlay.UpdateLayout();

        var topLeft = _tourTarget.TransformToVisual(TourOverlay).TransformPoint(new Point(0, 0));
        TourHighlight.Width = Math.Max(_tourTarget.ActualWidth, 1) + padding * 2;
        TourHighlight.Height = Math.Max(_tourTarget.ActualHeight, 1) + padding * 2;
        TourHighlight.RenderTransform = new TranslateTransform
        {
            X = topLeft.X - padding,
            Y = topLeft.Y - padding,
        };
    }

    private void TourOverlay_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (TourOverlay.Visibility == Visibility.Visible)
        {
            PositionTourHighlight();
        }
    }

    private void TourNextButton_Click(object sender, RoutedEventArgs e)
    {
        if (_tourIndex + 1 < _tourSteps.Length)
        {
            _tourIndex++;
            ShowTourStep();
        }
        else
        {
            CloseTour();
        }
    }

    private void TourSkipButton_Click(object sender, RoutedEventArgs e)
        => CloseTour();

    private void CloseTour()
    {
        _tourIndex = -1;
        _tourTarget = null;
        TourOverlay.Visibility = Visibility.Collapsed;
    }

    private void UpdateThemeToggleGlyph()
    {
        ThemeToggleIcon.Glyph = AppSettings.Current.Theme switch
        {
            "Dark" => "\uE708",
            "Light" => "\uE771",
            _ => "\uE770",
        };
    }

    private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        var current = AppSettings.Current.Theme;
        var next = current switch
        {
            "Dark" => "Light",
            "Light" => "Default",
            _ => "Dark",
        };

        AppSettings.Current.Theme = next;
        AppSettings.Current.Save();
        ViewModel.SetThemeCommand.Execute(next);

        UpdateThemeToggleGlyph();
    }

    private void CollectionItem_DragOver(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.Text))
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = Loc.T("DragToCollection");
        }
        else
        {
            e.AcceptedOperation = DataPackageOperation.None;
        }
    }

    private async void CollectionItem_Drop(object sender, DragEventArgs e)
    {
        if (sender is not FrameworkElement element
            || element.Tag is not long collectionId
            || !e.DataView.Contains(StandardDataFormats.Text))
        {
            return;
        }

        var pinIdText = await e.DataView.GetTextAsync();
        var pinIds = new List<long>();
        foreach (var part in pinIdText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (long.TryParse(part, out var id))
            {
                pinIds.Add(id);
            }
        }

        if (pinIds.Count > 0)
        {
            await ViewModel.AssignSelectedToCollectionAsync(pinIds.Distinct().ToList(), collectionId);
        }
    }
}