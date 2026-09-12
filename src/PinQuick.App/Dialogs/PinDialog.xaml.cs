using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using PinQuick.Core.Models;
using PinQuick.Core.Security;
using PinQuick.App.Services;
using PinQuick.App.ViewModels;

namespace PinQuick.App.Dialogs;

/// <summary>
/// Yeni pin oluşturma veya mevcut pini düzenleme iletişim kutusu.
/// </summary>
public sealed partial class PinDialog : ContentDialog
{
    public Pin Result { get; private set; } = null!;

    private readonly IReadOnlyList<Collection> _collections;

    public PinDialog(Pin? existing, IReadOnlyList<Collection> collections)
    {
        InitializeComponent();
        _collections = collections;

        TypeCombo.ItemsSource = Enum.GetValues<PinType>();
        CollectionCombo.ItemsSource = _collections;
        CollectionCombo.DisplayMemberPath = nameof(Collection.Name);
        IconGrid.ItemsSource = GlyphOptions.All;

        if (existing is null)
        {
            Title = Loc.T("NewPinDialogTitle");
            TypeCombo.SelectedItem = PinType.Application;
        }
        else
        {
            Title = Loc.T("EditPinDialogTitle");
            TitleBox.Text = existing.Title;
            TypeCombo.SelectedItem = existing.Type;
            TargetBox.Text = existing.Target;
            ArgumentsBox.Text = existing.Arguments;
            WorkingDirectoryBox.Text = existing.WorkingDirectory;
            DescriptionBox.Text = existing.Description;
            TagsBox.Text = existing.Tags;
            CollectionCombo.SelectedItem = _collections.FirstOrDefault(c => existing.CollectionIds.Contains(c.Id));
            FavoriteCheck.IsChecked = existing.IsFavorite;
            AdminCheck.IsChecked = existing.RunAsAdministrator;
            IconBox.Text = existing.Icon;
            _editing = existing;
        }

        PrimaryButtonClick += OnPrimaryButtonClick;
    }

    private Pin? _editing;

    private bool _missingTargetConfirmed;

    private void TargetBox_TextChanged(object sender, TextChangedEventArgs e) => _missingTargetConfirmed = false;

    private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TypeCombo.SelectedItem is not PinType type)
        {
            return;
        }

        TargetBox.Header = type switch
        {
            PinType.Website or PinType.Url => Loc.T("TargetHeaderUrl"),
            PinType.Command or PinType.PowerShell or PinType.Batch => Loc.T("TargetHeaderCommand"),
            PinType.WindowsSetting => Loc.T("TargetHeaderMsSettings"),
            PinType.NetworkPath => Loc.T("TargetHeaderNetwork"),
            PinType.SystemTool => Loc.T("TargetHeaderSystemTool"),
            _ => Loc.T("FieldTarget"),
        };

        BrowseButton.Visibility = type is PinType.Application or PinType.File
            or PinType.Folder or PinType.NetworkPath or PinType.Batch
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private async void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        if (TypeCombo.SelectedItem is not PinType type)
        {
            return;
        }

        if (type is PinType.Folder or PinType.NetworkPath)
        {
            var folderPicker = new FolderPicker(App.Window.AppWindow.Id)
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
            };

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder is not null)
            {
                TargetBox.Text = folder.Path;
            }

            return;
        }

        var filePicker = new FileOpenPicker(App.Window.AppWindow.Id)
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
        };
        if (type is PinType.Batch)
        {
            filePicker.FileTypeFilter.Add(".bat");
            filePicker.FileTypeFilter.Add(".cmd");
        }

        var file = await filePicker.PickSingleFileAsync();
        if (file is not null)
        {
            TargetBox.Text = file.Path;
        }
    }

    private void IconGrid_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is GlyphOption option)
        {
            IconBox.Text = option.Code;
            IconLibraryFlyout.Hide();
        }
    }

    private async void IconBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker(App.Window.AppWindow.Id)
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
        };
        picker.FileTypeFilter.Add(".ico");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".exe");

        var file = await picker.PickSingleFileAsync();
        if (file is not null)
        {
            IconBox.Text = file.Path;
        }
    }

    private void IconClearButton_Click(object sender, RoutedEventArgs e)
        => IconBox.Text = string.Empty;

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var title = TitleBox.Text?.Trim() ?? string.Empty;
        var target = TargetBox.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(title))
        {
            ShowError(Loc.T("ValidationTitleRequired"), args);
            return;
        }

        if (TypeCombo.SelectedItem is not PinType type)
        {
            ShowError(Loc.T("ValidationTypeRequired"), args);
            return;
        }

        if (string.IsNullOrWhiteSpace(target) && type is not PinType.Custom)
        {
            ShowError(Loc.T("ValidationTargetRequired"), args);
            return;
        }

        if (type is PinType.Website or PinType.Url && !PathValidation.IsValidUrl(target))
        {
            ShowError(Loc.T("ValidationInvalidUrl"), args);
            return;
        }

        if (type is PinType.WindowsSetting && !PathValidation.IsValidWindowsUri(target))
        {
            ShowError(Loc.T("ValidationInvalidMsSettings"), args);
            return;
        }

        if (type is PinType.NetworkPath
            && !target.StartsWith(@"\\", StringComparison.Ordinal)
            && !target.StartsWith("//", StringComparison.Ordinal))
        {
            ShowError(Loc.T("ValidationInvalidNetworkPath"), args);
            return;
        }

        if (type is PinType.Application or PinType.File or PinType.Folder
            or PinType.Batch or PinType.SystemTool or PinType.PowerShell)
        {
            var missing = PinHealth.IsBroken(new Pin { Type = type, Target = target });
            if (missing && !_missingTargetConfirmed)
            {
                _missingTargetConfirmed = true;
                ShowError(Loc.T("ValidationTargetMissing"), args);
                return;
            }
        }

        _missingTargetConfirmed = false;

        Result = new Pin
        {
            Id = _editing?.Id ?? 0,
            Title = title,
            Type = type,
            Target = target,
            Arguments = ArgumentsBox.Text?.Trim() ?? string.Empty,
            WorkingDirectory = WorkingDirectoryBox.Text?.Trim() ?? string.Empty,
            Icon = IconBox.Text?.Trim() ?? string.Empty,
            Description = DescriptionBox.Text?.Trim() ?? string.Empty,
            Tags = TagsBox.Text?.Trim() ?? string.Empty,
            IsFavorite = FavoriteCheck.IsChecked == true,
            RunAsAdministrator = AdminCheck.IsChecked == true,
            IsEnabled = _editing?.IsEnabled ?? true,
            CreatedAt = _editing?.CreatedAt ?? default,
            UpdatedAt = _editing?.UpdatedAt ?? default,
            LastUsedAt = _editing?.LastUsedAt,
            UseCount = _editing?.UseCount ?? 0,
            SortOrder = _editing?.SortOrder ?? 0,
            OpenWith = _editing?.OpenWith ?? string.Empty,
            CustomColor = _editing?.CustomColor ?? string.Empty,
            CustomShortcut = _editing?.CustomShortcut ?? string.Empty,
        };

        if (_editing is not null)
        {
            Result.CollectionIds.AddRange(_editing.CollectionIds);
        }

        if (CollectionCombo.SelectedItem is Collection selected && !Result.CollectionIds.Contains(selected.Id))
        {
            Result.CollectionIds.Add(selected.Id);
        }
    }

    private void ShowError(string message, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}