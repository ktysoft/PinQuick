using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PinQuick.App.Services;

namespace PinQuick.App.Dialogs;

public sealed partial class SettingsDialog : ContentDialog
{
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
    }

    private void ShowTourButton_Click(object sender, RoutedEventArgs e)
    {
        ShowTourRequested = true;
        Hide();
    }
}