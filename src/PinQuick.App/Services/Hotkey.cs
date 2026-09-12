using Windows.System;

namespace PinQuick.App.Services;

/// <summary>
/// Global kısayol kombinasyonlarının modeli, görselleştirmesi ve doğrulaması.
/// Modifier bitleri Windows RegisterHotKey uyumludur (MOD_ALT/MOD_CONTROL/MOD_SHIFT).
/// </summary>
internal static class Hotkey
{
    public const int ModAlt = 0x0001;
    public const int ModControl = 0x0002;
    public const int ModShift = 0x0004;
    public const int ModNoRepeat = 0x4000;

    public const int VkSpace = 0x20;

    /// <summary>
    /// Yalnızca modifier tuşlarından oluşan basışlar kısayol sayılmaz; ana tuş gerekir.
    /// </summary>
    public static bool IsModifierKey(VirtualKey key)
        => key is VirtualKey.Control
            or VirtualKey.Shift
            or VirtualKey.Menu
            or VirtualKey.LeftControl
            or VirtualKey.RightControl
            or VirtualKey.LeftShift
            or VirtualKey.RightShift
            or VirtualKey.LeftMenu
            or VirtualKey.RightMenu;

    /// <summary>
    /// Kombinasyonun okunabilir metnini üretir, ör: "CTRL + SHIFT + S".
    /// Ana tuş atanmamışsa yalnızca modifier'lar gösterilir.
    /// </summary>
    public static string Format(int modifiers, int vk)
    {
        var parts = new List<string>();
        if ((modifiers & ModControl) != 0)
        {
            parts.Add(Loc.T("HotkeyModifierCtrl"));
        }

        if ((modifiers & ModShift) != 0)
        {
            parts.Add(Loc.T("HotkeyModifierShift"));
        }

        if ((modifiers & ModAlt) != 0)
        {
            parts.Add(Loc.T("HotkeyModifierAlt"));
        }

        if (vk != 0)
        {
            parts.Add(KeyName(vk));
        }

        return string.Join(" + ", parts);
    }

    /// <summary>
    /// Sanal tuş kodunun görünen adını döndürür. Harf, rakam ve F tuşları için
    /// doğrudan ad; özel tuşlar için sabit (İngilizce) tanımlar kullanılır.
    /// </summary>
    public static string KeyName(int vk)
    {
        return vk switch
        {
            >= 0x30 and <= 0x39 => ((char)('0' + (vk - 0x30))).ToString(),
            >= 0x41 and <= 0x5A => ((char)('A' + (vk - 0x41))).ToString(),
            >= 0x60 and <= 0x69 => ((char)('0' + (vk - 0x60))).ToString(),
            >= 0x70 and <= 0x87 => $"F{vk - 0x70 + 1}",
            0x20 => "Space",
            0x0D => "Enter",
            0x09 => "Tab",
            0x1B => "Esc",
            0x08 => "Backspace",
            0x2D => "Insert",
            0x25 => "Left",
            0x26 => "Up",
            0x27 => "Right",
            0x28 => "Down",
            0x24 => "Home",
            0x23 => "End",
            0x21 => "PageUp",
            0x22 => "PageDown",
            0x2E => "Delete",
            0x6A => "*",
            0x6B => "+",
            0x6D => "-",
            0x6E => ".",
            0x6F => "/",
            _ => FormatFallback(vk),
        };
    }

    private static string FormatFallback(int vk)
    {
        try
        {
            return ((VirtualKey)vk).ToString();
        }
        catch
        {
            return vk.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}