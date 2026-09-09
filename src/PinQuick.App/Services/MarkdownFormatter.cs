using System.Text.RegularExpressions;

namespace PinQuick.App.Services;

/// <summary>
/// Pin açıklamalarında temel Markdown biçimlendirmesi sağlar.
/// Kalın, italik, kod, bağlantı ve başlık işaretlerini işler.
/// </summary>
public static partial class MarkdownFormatter
{
    [GeneratedRegex(@"\*\*(.+?)\*\*", RegexOptions.Compiled)]
    private static partial Regex BoldPattern();

    [GeneratedRegex(@"\*(.+?)\*", RegexOptions.Compiled)]
    private static partial Regex ItalicPattern();

    [GeneratedRegex(@"`(.+?)`", RegexOptions.Compiled)]
    private static partial Regex CodePattern();

    [GeneratedRegex(@"\[(.+?)\]\((https?://[^\s)]+)\)", RegexOptions.Compiled)]
    private static partial Regex LinkPattern();

    [GeneratedRegex(@"^#{1,6}\s+", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex HeadingPattern();

    [GeneratedRegex(@"^\s*(?:[-*+]|\d+\.)\s+", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex ListPattern();

    /// <summary>
    /// Markdown metnini düz metne dönüştürür. Ayrıştırılamayan
    /// işaretler orijinal halleriyle korunur.
    /// </summary>
    public static string ToPlainText(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var text = markdown;
        text = LinkPattern().Replace(text, m => $"{m.Groups[1].Value} ({m.Groups[2].Value})");
        text = BoldPattern().Replace(text, "$1");
        text = ItalicPattern().Replace(text, "$1");
        text = CodePattern().Replace(text, "$1");
        text = HeadingPattern().Replace(text, string.Empty);
        text = ListPattern().Replace(text, "• ");
        text = text.Replace("---", string.Empty, StringComparison.Ordinal);
        return text.Trim();
    }

    /// <summary>
    /// İlk satırda veya kısa açıklamalarda özet göstermek için
    /// kullanılabilecek kısaltılmış metni döndürür.
    /// </summary>
    public static string GetPreview(string? markdown, int maxLength = 120)
    {
        var text = ToPlainText(markdown);
        if (text.Length <= maxLength)
        {
            return text;
        }

        var cut = text.AsSpan(0, maxLength);
        var lastSpace = cut.LastIndexOf(' ');
        return lastSpace > 0
            ? $"{text[..lastSpace]}…"
            : $"{text[..maxLength]}…";
    }
}