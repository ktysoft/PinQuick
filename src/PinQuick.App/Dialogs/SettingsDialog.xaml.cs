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
    }
}