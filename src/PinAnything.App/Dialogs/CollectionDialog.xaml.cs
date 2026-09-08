using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PinAnything_App.Dialogs;

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
            ErrorText.Text = "Koleksiyon adı boş olamaz.";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        ResultName = name;
    }
}