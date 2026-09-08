using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Windows.Storage.Pickers;
using PinAnything_App.Dialogs;
using PinAnything_App.ViewModels;
using PinAnything.Core.Models;
using PinAnything.Core.Services;

namespace PinAnything_App;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        InitializeComponent();

        ViewModel = new MainViewModel(App.Services.PinManager, App.Services.CollectionManager, App.Services.ProcessLauncher);
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        ViewModel.Pins.CollectionChanged += (_, _) => UpdateEmptyState();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadCommand.ExecuteAsync(null);
        UpdateEmptyState();
        SearchBox.Focus(FocusState.Programmatic);
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedPin))
        {
            RefreshDetailPanel(ViewModel.SelectedPin);
        }
        else if (e.PropertyName == nameof(MainViewModel.RequestedTheme))
        {
            UpdateThemeCombo();
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

    private void RefreshDetailPanel(PinItemViewModel? pin)
    {
        if (pin is null)
        {
            DetailPanel.Visibility = Visibility.Collapsed;
            return;
        }

        DetailIcon.Glyph = pin.IconGlyph;
        DetailTitle.Text = pin.Title;
        DetailTypeDisplay.Text = pin.TypeDisplay;
        DetailTarget.Text = pin.Target;

        if (!string.IsNullOrEmpty(pin.Description))
        {
            DetailDescription.Text = pin.Description;
            DetailDescription.Visibility = Visibility.Visible;
        }
        else
        {
            DetailDescription.Visibility = Visibility.Collapsed;
        }

        if (!string.IsNullOrEmpty(pin.Tags))
        {
            DetailTags.Text = $"Etiketler: {pin.Tags}";
            DetailTags.Visibility = Visibility.Visible;
        }
        else
        {
            DetailTags.Visibility = Visibility.Collapsed;
        }

        DetailPanel.Visibility = Visibility.Visible;
    }

    private void UpdateThemeCombo()
    {
        var theme = ViewModel.RequestedTheme;
        ThemeCombo.SelectedIndex = theme switch
        {
            ElementTheme.Dark => 1,
            ElementTheme.Light => 2,
            _ => 0,
        };
    }

    private void ThemeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var value = (ThemeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();
        if (value is null)
        {
            return;
        }

        ViewModel.SetThemeCommand.Execute(value switch
        {
            "Koyu" => "Dark",
            "Açık" => "Light",
            _ => "System",
        });
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
            await ViewModel.AddCollectionAsync(dialog.ResultName);
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
            Title = "Koleksiyonu sil",
            Content = $"'{ViewModel.SelectedCollection.Name}' silinsin mi? İçindeki pinler korunur.",
            PrimaryButtonText = "Sil",
            CloseButtonText = "İptal",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = RootGrid.XamlRoot,
        };

        if (await confirm.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteCollectionAsync(ViewModel.SelectedCollection);
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
        if (ViewModel.SelectedPin is null)
        {
            return;
        }

        var confirm = new ContentDialog
        {
            Title = "Pini sil",
            Content = $"'{ViewModel.SelectedPin.Title}' silinsin mi?",
            PrimaryButtonText = "Sil",
            CloseButtonText = "İptal",
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
        if (e.ClickedItem is PinItemViewModel item)
        {
            ViewModel.SelectedPin = item;
        }
    }

    private async void ExportMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileSavePicker(App.Window.AppWindow.Id)
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = "pin-anything-pins",
        };
        picker.FileTypeChoices.Add("JSON", new List<string> { ".json" });

        var file = await picker.PickSaveFileAsync();
        if (file is not null)
        {
            await ViewModel.ExportAsync(file.Path);
        }
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

    private async Task ShowErrorAsync(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Hata",
            Content = message,
            CloseButtonText = "Tamam",
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
}