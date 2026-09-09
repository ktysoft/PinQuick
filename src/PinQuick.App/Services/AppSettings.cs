using System.Text.Json;
using System.Text.Json.Serialization;

namespace PinQuick.App.Services;

/// <summary>
/// Kalıcı uygulama ayarları. %LOCALAPPDATA%\PinQuick\settings.json içinde tutulur.
/// </summary>
[JsonSerializable(typeof(AppSettings))]
internal sealed partial class AppSettingsJsonContext : JsonSerializerContext;
public sealed class AppSettings
{
    private const string DefaultTheme = "Default";
    private const string DefaultLanguage = "tr";

    public string Theme { get; set; } = DefaultTheme;

    public string Language { get; set; } = DefaultLanguage;

    public bool RunAtStartup { get; set; }

    public string CardSize { get; set; } = "Medium";

    public bool MinimizeToTray { get; set; }

    public bool GlobalHotkeyEnabled { get; set; }

    public bool IsOnboardingCompleted { get; set; }

    public int? WindowWidth { get; set; }

    public int? WindowHeight { get; set; }

    public bool WindowMaximized { get; set; }

    public int? WindowPositionX { get; set; }

    public int? WindowPositionY { get; set; }

    public string AutoBackupFrequency { get; set; } = "Off";

    public DateTime? LastAutoBackupAt { get; set; }

    public List<string> SearchHistory { get; set; } = new();

    private static AppSettings? _instance;

    public static AppSettings Current => _instance ??= Load();

    public static string SettingsPath => Path.Combine(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PinQuick"),
        "settings.json");

    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, AppSettingsJsonContext.Default.AppSettings));
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var settings = JsonSerializer.Deserialize(
                    File.ReadAllText(SettingsPath),
                    AppSettingsJsonContext.Default.AppSettings);
                if (settings is not null)
                {
                    settings.RunAtStartup = StartupManager.IsEnabled();
                    return settings;
                }
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return new AppSettings { RunAtStartup = StartupManager.IsEnabled() };
    }

    internal static void Reset() => _instance = null;
}