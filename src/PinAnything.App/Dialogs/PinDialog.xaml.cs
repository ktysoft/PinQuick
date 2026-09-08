using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PinAnything.Core.Models;
using PinAnything_App.ViewModels;

namespace PinAnything_App.Dialogs;

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

        if (existing is null)
        {
            Title = "Yeni Pin";
            TypeCombo.SelectedItem = PinType.Application;
        }
        else
        {
            Title = "Pini Düzenle";
            TitleBox.Text = existing.Title;
            TypeCombo.SelectedItem = existing.Type;
            TargetBox.Text = existing.Target;
            ArgumentsBox.Text = existing.Arguments;
            WorkingDirectoryBox.Text = existing.WorkingDirectory;
            DescriptionBox.Text = existing.Description;
            TagsBox.Text = existing.Tags;
            CollectionCombo.SelectedItem = _collections.FirstOrDefault(c => c.Id == existing.CollectionId);
            FavoriteCheck.IsChecked = existing.IsFavorite;
            AdminCheck.IsChecked = existing.RunAsAdministrator;
            _editing = existing;
        }

        PrimaryButtonClick += OnPrimaryButtonClick;
    }

    private Pin? _editing;

    private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TypeCombo.SelectedItem is not PinType type)
        {
            return;
        }

        TargetBox.Header = type switch
        {
            PinType.Website or PinType.Url => "Hedef (URL)",
            PinType.Command or PinType.PowerShell or PinType.Batch => "Komut",
            PinType.WindowsSetting => "Hedef (ms-settings:...)",
            PinType.NetworkPath => "Hedef (\\\\sunucu\\paylaşım)",
            PinType.SystemTool => "Hedef (devmgmt.msc vb.)",
            _ => "Hedef",
        };
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var title = TitleBox.Text?.Trim() ?? string.Empty;
        var target = TargetBox.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(title))
        {
            ShowError("Başlık boş olamaz.", args);
            return;
        }

        if (TypeCombo.SelectedItem is not PinType type)
        {
            ShowError("Tür seçilmelidir.", args);
            return;
        }

        if (string.IsNullOrWhiteSpace(target) && type is not PinType.Custom)
        {
            ShowError("Hedef boş olamaz.", args);
            return;
        }

        Result = new Pin
        {
            Id = _editing?.Id ?? 0,
            Title = title,
            Type = type,
            Target = target,
            Arguments = ArgumentsBox.Text?.Trim() ?? string.Empty,
            WorkingDirectory = WorkingDirectoryBox.Text?.Trim() ?? string.Empty,
            Description = DescriptionBox.Text?.Trim() ?? string.Empty,
            Tags = TagsBox.Text?.Trim() ?? string.Empty,
            CollectionId = (CollectionCombo.SelectedItem as Collection)?.Id,
            IsFavorite = FavoriteCheck.IsChecked == true,
            RunAsAdministrator = AdminCheck.IsChecked == true,
            IsEnabled = _editing?.IsEnabled ?? true,
            CreatedAt = _editing?.CreatedAt ?? default,
            UpdatedAt = _editing?.UpdatedAt ?? default,
            LastUsedAt = _editing?.LastUsedAt,
            UseCount = _editing?.UseCount ?? 0,
            SortOrder = _editing?.SortOrder ?? 0,
        };
    }

    private void ShowError(string message, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}