using System.Globalization;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace PinQuick.App.Converters;

/// <summary>
/// "#RRGGBB" biçimindeki hex renk dizesini <see cref="SolidColorBrush"/> değerine dönüştürür.
/// </summary>
public sealed class HexToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, string language)
    {
        if (value is not string hex || string.IsNullOrWhiteSpace(hex))
        {
            return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }

        return new SolidColorBrush(ParseHexColor(hex));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();

    public static Color ParseHexColor(string hex)
    {
        var cleaned = hex.Trim().TrimStart('#');
        if (cleaned.Length == 6
            && byte.TryParse(cleaned.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r)
            && byte.TryParse(cleaned.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g)
            && byte.TryParse(cleaned.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
        {
            return Color.FromArgb(255, r, g, b);
        }

        return Microsoft.UI.Colors.Transparent;
    }
}