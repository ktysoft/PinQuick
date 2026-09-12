using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using PinQuick.App.Services;
using Windows.Graphics;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PinQuick.App;

/// <summary>
/// The application window. This hosts a Frame that displays pages. Add your
/// UI and logic to MainPage.xaml / MainPage.xaml.cs instead of here so you
/// can use Page features such as navigation events and the Loaded lifecycle.
/// </summary>
public sealed partial class MainWindow : Window
{
    private const int DwmwaUseImmersiveDarkMode = 20;

    private NativeWindowBridge? _native;
    private bool _isExitingFromTray;
    private DispatcherTimer? _backupTimer;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int attributeValue, int attributeSize);

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        AppWindow.SetIcon("Assets/AppIcon.ico");

        var title = $"{AppInfo.Title} {AppInfo.Version}";
        Title = title;
        AppTitleBar.Title = title;

        ApplyTheme(AppSettings.Current.Theme switch
        {
            "Light" => ElementTheme.Light,
            "Dark" => ElementTheme.Dark,
            _ => ElementTheme.Default,
        });

        ApplyPersistedWindowState();
        Closed += OnClosed;
        AppWindow.Closing += OnAppWindowClosing;
        AppWindow.Changed += OnAppWindowChanged;
        InitializeAutoBackup();

        // Navigate the root frame to the main page on startup.
        RootFrame.Navigate(typeof(MainPage));
    }

    /// <summary>
    /// Pencere HWND'si hazır olduktan sonra tepsisi ve global kısayolu başlatır.
    /// </summary>
    public void InitializeNativeBridge()
    {
        if (_native is not null)
        {
            return;
        }

        var bridge = new NativeWindowBridge(WindowNative.GetWindowHandle(this));
        bridge.ToggleWindowRequested += OnToggleWindowRequested;
        bridge.ExitRequested += OnExitRequested;
        _native = bridge;
        ApplyNativeSettings();
    }

    /// <summary>
    /// Tepsi ikonu ve global kısayol davranışını mevcut ayarlara göre günceller.
    /// Global kısayol kaydedilemezse (başka programda kullanımda) false döner.
    /// </summary>
    public bool ApplyNativeSettings()
        => _native?.ApplySettings(
            AppSettings.Current.MinimizeToTray,
            AppSettings.Current.GlobalHotkeyEnabled,
            AppSettings.Current.HotkeyModifiers,
            AppSettings.Current.HotkeyKey) ?? true;

    private void OnAppWindowClosing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (!AppSettings.Current.MinimizeToTray || _isExitingFromTray)
        {
            return;
        }

        args.Cancel = true;
        sender.Hide();
    }

    private void OnAppWindowChanged(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowChangedEventArgs args)
    {
        if (!AppSettings.Current.MinimizeToTray || _isExitingFromTray)
        {
            return;
        }

        if (args.DidPresenterChange
            && sender.Presenter is OverlappedPresenter presenter
            && presenter.State == OverlappedPresenterState.Minimized)
        {
            sender.Hide();
        }
    }

    private void OnToggleWindowRequested()
    {
        if (AppWindow.IsVisible)
        {
            AppWindow.Hide();
        }
        else
        {
            if (AppWindow.Presenter is OverlappedPresenter presenter && presenter.State != OverlappedPresenterState.Maximized)
            {
                presenter.Restore();
            }

            AppWindow.Show();
            NativeWindowBridge.BringToFront(WindowNative.GetWindowHandle(this));
            Activate();
        }
    }

    private void OnExitRequested()
    {
        _isExitingFromTray = true;
        Close();
    }

    private void ApplyPersistedWindowState()
    {
        try
        {
            var settings = AppSettings.Current;

            if (settings.WindowWidth is > 0 && settings.WindowHeight is > 0)
            {
                AppWindow.Resize(new SizeInt32(settings.WindowWidth.Value, settings.WindowHeight.Value));
            }

            if (settings.WindowPositionX is int x && settings.WindowPositionY is int y)
            {
                AppWindow.Move(new PointInt32(x, y));
            }

            if (settings.WindowMaximized && AppWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.Maximize();
            }
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException)
        {
            // Pencere durumu uygulanamazsa varsayılan boyut kullanılır.
        }
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        SaveWindowState();
        _backupTimer?.Stop();
        _native?.Dispose();
    }

    private void SaveWindowState()
    {
        try
        {
            var settings = AppSettings.Current;

            if (AppWindow.Presenter is OverlappedPresenter presenter)
            {
                if (presenter.State == OverlappedPresenterState.Minimized)
                {
                    return;
                }

                settings.WindowMaximized = presenter.State == OverlappedPresenterState.Maximized;
            }

            settings.WindowWidth = AppWindow.Size.Width;
            settings.WindowHeight = AppWindow.Size.Height;
            settings.WindowPositionX = AppWindow.Position.X;
            settings.WindowPositionY = AppWindow.Position.Y;
            settings.Save();
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException)
        {
            // Pencere kapanırken ölçü alınamazsa mevcut ayar korunur.
        }
    }

    private void InitializeAutoBackup()
    {
        _backupTimer = new DispatcherTimer { Interval = TimeSpan.FromHours(1) };
        _backupTimer.Tick += OnBackupTimerTick;
        _backupTimer.Start();
    }

    private void OnBackupTimerTick(object? sender, object e)
    {
        var settings = AppSettings.Current;
        if (settings.AutoBackupFrequency == "Off")
        {
            return;
        }

        var interval = settings.AutoBackupFrequency == "Daily" ? TimeSpan.FromDays(1) : TimeSpan.FromDays(7);
        var last = settings.LastAutoBackupAt;
        var due = last is null || DateTime.UtcNow - last.Value >= interval;

        if (!due)
        {
            return;
        }

        try
        {
            BackupService.CreateBackup();
            settings.LastAutoBackupAt = DateTime.UtcNow;
            settings.Save();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }

    public void ApplyTheme(ElementTheme theme)
    {
        RootLayout.RequestedTheme = theme;
        int darkMode = theme switch
        {
            ElementTheme.Dark => 1,
            ElementTheme.Light => 0,
            _ => IsSystemDarkTheme() ? 1 : 0,
        };

        var hwnd = WindowNative.GetWindowHandle(this);
        _ = DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref darkMode, sizeof(int));
    }

    private static bool IsSystemDarkTheme()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int value && value == 0;
        }
        catch
        {
            return false;
        }
    }
}
