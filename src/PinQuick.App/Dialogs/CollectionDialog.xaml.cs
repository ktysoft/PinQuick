using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PinQuick.App.Services;

namespace PinQuick.App.Dialogs;

/// <summary>
/// Koleksiyon oluşturma iletişim kutusu.
/// </summary>
public sealed partial class CollectionDialog : ContentDialog
{
    public string? ResultName { get; private set; }

    public CollectionDialog()
    {
        InitializeComponent();
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