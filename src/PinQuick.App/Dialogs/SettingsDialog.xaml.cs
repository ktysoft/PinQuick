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
    public string SelectedTheme => ThemeCombo.SelectedIndex == 0 ? "Dark" : "Light";

    public string SelectedLanguage => LanguageCombo.SelectedIndex == 1 ? "en" : "tr";

    public bool StartupEnabled => StartupToggle.IsOn;

    public bool MinimizeToTrayEnabled => MinimizeToTrayToggle.IsOn;

    public bool GlobalHotkeyEnabled => GlobalHotkeyToggle.IsOn;

    public int HotkeyModifiers => CurrentModifiers();

    public int HotkeyKey => _hotkeyVk;

    public string SelectedAutoBackup => AutoBackupCombo.SelectedIndex switch
    {
        1 => "Daily",
        2 => "Weekly",
        _ => "Off",
    };

    public bool ShowTourRequested { get; private set; }

    private int _hotkeyVk;

    public SettingsDialog()
    {
        InitializeComponent();
        ThemeCombo.SelectedIndex = AppSettings.Current.Theme == "Dark" ? 0 : 1;
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

        ModControlButton.IsChecked = (AppSettings.Current.HotkeyModifiers & Hotkey.ModControl) != 0;
        ModShiftButton.IsChecked = (AppSettings.Current.HotkeyModifiers & Hotkey.ModShift) != 0;
        ModAltButton.IsChecked = (AppSettings.Current.HotkeyModifiers & Hotkey.ModAlt) != 0;
        _hotkeyVk = AppSettings.Current.HotkeyKey;

        HotkeyEditor.Opacity = AppSettings.Current.GlobalHotkeyEnabled ? 1.0 : 0.5;
        HotkeyEditor.IsHitTestVisible = AppSettings.Current.GlobalHotkeyEnabled;
        UpdateHotkeyDisplay();
    }

    private int CurrentModifiers()
    {
        var modifiers = 0;
        if (ModControlButton.IsChecked == true)
        {
            modifiers |= Hotkey.ModControl;
        }

        if (ModShiftButton.IsChecked == true)
        {
            modifiers |= Hotkey.ModShift;
        }

        if (ModAltButton.IsChecked == true)
        {
            modifiers |= Hotkey.ModAlt;
        }

        return modifiers;
    }

    private void SetHotkeyEditorEnabled()
    {
        var enabled = GlobalHotkeyToggle.IsOn;
        HotkeyEditor.Opacity = enabled ? 1.0 : 0.5;
        HotkeyEditor.IsHitTestVisible = enabled;
    }

    private void Dialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (!GlobalHotkeyToggle.IsOn || _hotkeyVk != 0)
        {
            return;
        }

        args.Cancel = true;
        ShowHint(Loc.T("HotkeyErrorModifierOnly"));
    }

    private void GlobalHotkeyToggle_Toggled(object sender, RoutedEventArgs e)
    {
        SetHotkeyEditorEnabled();
        HideHint();
    }

    private void ModifierToggle_Checked(object sender, RoutedEventArgs e)
        => UpdateHotkeyDisplay();

    private void ModifierToggle_Unchecked(object sender, RoutedEventArgs e)
        => UpdateHotkeyDisplay();

    private void HotkeyInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        e.Handled = true;
        HideHint();

        if (IsWindowsKeyDown())
        {
            return;
        }

        if (Hotkey.IsModifierKey(e.Key))
        {
            ShowHint(Loc.T("HotkeyErrorModifierOnly"));
            return;
        }

        _hotkeyVk = (int)e.Key;
        UpdateHotkeyDisplay();
    }

    private void UpdateHotkeyDisplay()
    {
        HotkeyInput.Text = Hotkey.Format(CurrentModifiers(), _hotkeyVk);
    }

    private void ShowHint(string message)
    {
        HotkeyHint.Text = message;
        HotkeyHint.Visibility = Visibility.Visible;
    }

    private void HideHint()
        => HotkeyHint.Visibility = Visibility.Collapsed;

    private static bool IsWindowsKeyDown()
    {
        var keyStates = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.LeftWindows);
        var rightStates = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.RightWindows);
        return keyStates.HasFlag(CoreVirtualKeyStates.Down)
            || rightStates.HasFlag(CoreVirtualKeyStates.Down);
    }

    private void ShowTourButton_Click(object sender, RoutedEventArgs e)
    {
        ShowTourRequested = true;
        Hide();
    }
}