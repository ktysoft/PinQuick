using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using PinQuick.App.Services;

namespace PinQuick.App;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// The main application window. Use <c>App.Window</c> from any class that needs
    /// the window reference (for dialogs, pickers, interop, etc.).
    /// </summary>
    public static Window Window { get; private set; } = null!;

    /// <summary>
    /// The UI thread dispatcher. Use <c>App.DispatcherQueue</c> to marshal calls
    /// to the UI thread. Fully qualified to avoid CS0104 ambiguity with
    /// <see cref="Windows.System.DispatcherQueue"/>.
    /// </summary>
    public static Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; private set; } = null!;

    /// <summary>
    /// The native window handle (HWND). Use for file pickers,
    /// <c>DataTransferManager</c>, and any WinRT interop that requires
    /// <c>InitializeWithWindow</c>.
    /// </summary>
    public static nint WindowHandle =>
        WinRT.Interop.WindowNative.GetWindowHandle(Window);

    public static AppServices Services { get; } = new();

    private const string SingleInstanceMutexName = @"Local\PinQuick.SingleInstance";

    private static Mutex? _singleInstanceMutex;

    public App()
    {
        InitializeComponent();
        Loc.Language = AppSettings.Current.Language;
        UnhandledException += (s, e) =>
        {
            try
            {
                File.AppendAllText(
                    Path.Combine(Path.GetTempPath(), "pinquick-crash.log"),
                    $"[{DateTime.Now:O}] {e.Exception}\n");
            }
            catch
            {
            }
        };
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        if (!TryAcquireSingleInstance())
        {
            return;
        }

        Window = new MainWindow();
        DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        Window.Activate();

        if (Window is MainWindow mainWindow)
        {
            mainWindow.InitializeNativeBridge();
        }
    }

    private static bool TryAcquireSingleInstance()
    {
        _singleInstanceMutex = new Mutex(
            initiallyOwned: true,
            name: SingleInstanceMutexName,
            createdNew: out var isFirstInstance);

        if (isFirstInstance)
        {
            return true;
        }

        ActivateExistingInstance();
        Environment.Exit(0);
        return false;
    }

    private static void ActivateExistingInstance()
    {
        try
        {
            var title = $"{AppInfo.Title} {AppInfo.Version}";
            var hwnd = FindWindowW(null, title);
            if (hwnd == IntPtr.Zero)
            {
                return;
            }

            ShowWindow(hwnd, 9); // SW_RESTORE
            SetForegroundWindow(hwnd);
        }
        catch
        {
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindowW(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public static void Restart()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule?.FileName ?? Path.Combine(AppContext.BaseDirectory, "PinQuick.App.exe"),
                UseShellExecute = true,
            });
        }
        catch
        {
        }

        Environment.Exit(0);
    }
}
