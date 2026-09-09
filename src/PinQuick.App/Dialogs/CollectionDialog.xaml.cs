using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PinQuick.App.Services;

namespace PinQuick.App.Dialogs;

/// <summary>
/// Koleksiyon oluşturma veya yeniden adlandırma iletişim kutusu.
/// </summary>
public sealed partial class CollectionDialog : ContentDialog
{
    public string? ResultName { get; private set; }

    public CollectionDialog(string? existingName = null)
    {
        InitializeComponent();

        if (existingName is not null)
        {
            Title = Loc.T("EditCollectionTitle");
            PrimaryButtonText = Loc.T("SaveButton");
            NameBox.Text = existingName;
        }

        PrimaryButtonClick += OnPrimaryButtonClick;
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var name = NameBox.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            args.Cancel = true;
            ErrorText.Text = Loc.T("ValidationCollectionNameRequired");
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        ResultName = name;
    }
}