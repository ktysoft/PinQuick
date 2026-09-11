using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PinQuick.App.Services;
using Windows.System;
using Windows.UI.Core;

namespace PinQuick.App.Dialogs;

public sealed partial class SettingsDialog : ContentDialog
{
    private uint _hotkeyModifiers;
    private uint _hotkeyVirtualKey;

    public string SelectedTheme => ThemeCombo.SelectedIndex switch
    {
        1 => "Dark",
        2 => "Light",
        _ => "Default",
    };

    public string SelectedLanguage => LanguageCombo.SelectedIndex == 1 ? "en" : "tr";

    public bool StartupEnabled => StartupToggle.IsOn;

    public bool MinimizeToTrayEnabled => MinimizeToTrayToggle.IsOn;

    public bool GlobalHotkeyEnabled => GlobalHotkeyToggle.IsOn;

    public string SelectedGlobalHotkey => NativeWindowBridge.FormatHotkey(_hotkeyModifiers, _hotkeyVirtualKey);

    public string SelectedAutoBackup => AutoBackupCombo.SelectedIndex switch
    {
        1 => "Daily",
        2 => "Weekly",
        _ => "Off",
    };

    public bool ShowTourRequested { get; private set; }

    public SettingsDialog()
    {
        InitializeComponent();
        ThemeCombo.SelectedIndex = AppSettings.Current.Theme switch
        {
            "Dark" => 1,
            "Light" => 2,
            _ => 0,
        };
        LanguageCombo.SelectedIndex = Loc.Language == "en" ? 1 : 0;
        StartupToggle.IsOn = AppSettings.Current.RunAtStartup;
        MinimizeToTrayToggle.IsOn = AppSettings.Current.MinimizeToTray;
        GlobalHotkeyToggle.IsOn = AppSettings.Current.GlobalHotkeyEnabled;
        AutoBackupCombo.SelectedIndex = AppSettings.Current.AutoBackupFrequency switch
        {
            "Daily" => 1,
            "Weekly" => 2,
            _ => 0,
        };

        var (modifiers, virtualKey) = NativeWindowBridge.TryParseHotkey(AppSettings.Current.GlobalHotkey, out var m, out var v)
            ? (m, v)
            : (NativeWindowBridge.ModControl, NativeWindowBridge.VkSpace);
        _hotkeyModifiers = modifiers;
        _hotkeyVirtualKey = virtualKey;
        HotkeyBox.Text = NativeWindowBridge.FormatHotkey(_hotkeyModifiers, _hotkeyVirtualKey);
        HotkeyBox.IsEnabled = GlobalHotkeyToggle.IsOn;
        GlobalHotkeyToggle.Toggled += (_, _) => HotkeyBox.IsEnabled = GlobalHotkeyToggle.IsOn;
    }

    private void HotkeyBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        e.Handled = true;

        if (e.Key is VirtualKey.Escape or VirtualKey.Back)
        {
            _hotkeyModifiers = NativeWindowBridge.ModControl;
            _hotkeyVirtualKey = NativeWindowBridge.VkSpace;
            HotkeyBox.Text = NativeWindowBridge.FormatHotkey(_hotkeyModifiers, _hotkeyVirtualKey);
            HotkeyWarning.Visibility = Visibility.Collapsed;
            return;
        }

        if (e.Key is VirtualKey.Control or VirtualKey.Shift or VirtualKey.Menu
            or VirtualKey.LeftWindows or VirtualKey.RightWindows)
        {
            return;
        }

        var modifiers = ReadModifiers();
        if (modifiers == 0)
        {
            HotkeyWarning.Visibility = Visibility.Visible;
            return;
        }

        _hotkeyModifiers = modifiers;
        _hotkeyVirtualKey = (uint)e.Key;
        HotkeyBox.Text = NativeWindowBridge.FormatHotkey(_hotkeyModifiers, _hotkeyVirtualKey);
        HotkeyWarning.Visibility = Visibility.Collapsed;
    }

    private static uint ReadModifiers()
    {
        var modifiers = 0u;
        if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down))
        {
            modifiers |= NativeWindowBridge.ModControl;
        }
        if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down))
        {
            modifiers |= NativeWindowBridge.ModAlt;
        }
        if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down))
        {
            modifiers |= NativeWindowBridge.ModShift;
        }
        if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.LeftWindows).HasFlag(CoreVirtualKeyStates.Down))
        {
            modifiers |= NativeWindowBridge.ModWin;
        }
        return modifiers;
    }

    private void ShowTourButton_Click(object sender, RoutedEventArgs e)
    {
        ShowTourRequested = true;
        Hide();
    }
}