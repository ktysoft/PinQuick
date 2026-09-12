using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PinQuick.App.Converters;
using PinQuick.App.Services;
using Windows.UI;

namespace PinQuick.App.Dialogs;

/// <summary>
/// Koleksiyon oluşturma veya yeniden adlandırma iletişim kutusu.
/// </summary>
public sealed partial class CollectionDialog : ContentDialog
{
    private const string NoColor = "";

    /// <summary>
    /// Koleksiyonlara önerilen hazır renkler. Sağ tık menüsündeki "Renk Seç" de bunları kullanır.
    /// </summary>
    public static readonly string[] PresetColors =
    [
        "#E81123", "#FF8C00", "#FCE100", "#107C10", "#0078D4",
        "#5C2D91", "#E3008C", "#00B294", "#6B69D6", "#767676",
    ];

    public string? ResultName { get; private set; }

    public string ResultColor { get; private set; } = NoColor;

    public CollectionDialog(string? existingName = null, string? existingColor = null)
    {
        InitializeComponent();

        if (existingName is not null)
        {
            Title = Loc.T("EditCollectionTitle");
            PrimaryButtonText = Loc.T("SaveButton");
            NameBox.Text = existingName;
        }

        if (string.IsNullOrWhiteSpace(existingColor))
        {
            NoColorToggle.IsChecked = true;
            ColorPreview.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }
        else
        {
            ResultColor = existingColor.Trim();
            var color = HexToBrushConverter.ParseHexColor(existingColor);
            ColorPickerBox.Color = color;
            ColorPreview.Background = new SolidColorBrush(color);
        }

        BuildPresetSwatches();
        NameBox.Loaded += (_, _) => NameBox.Focus(FocusState.Programmatic);

        PrimaryButtonClick += OnPrimaryButtonClick;
    }

    private void BuildPresetSwatches()
    {
        foreach (var hex in PresetColors)
        {
            var swatch = new Button
            {
                Width = 28,
                Height = 28,
                Padding = new Thickness(0),
                CornerRadius = new CornerRadius(6),
                Tag = hex,
                Background = new SolidColorBrush(HexToBrushConverter.ParseHexColor(hex)),
            };
            ToolTipService.SetToolTip(swatch, hex);
            swatch.Click += PresetColorButton_Click;
            SwatchContainer.Children.Add(swatch);
        }
    }

    private void PresetColorButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string hex })
        {
            return;
        }

        ColorPickerBox.Color = HexToBrushConverter.ParseHexColor(hex);
    }

    private void ColorPickerBox_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
    {
        var color = args.NewColor;
        ResultColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        ColorPreview.Background = new SolidColorBrush(color);
        NoColorToggle.IsChecked = false;
    }

    private void NoColorToggle_Click(object sender, RoutedEventArgs e)
    {
        if (NoColorToggle.IsChecked == true)
        {
            ResultColor = NoColor;
            ColorPreview.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }
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