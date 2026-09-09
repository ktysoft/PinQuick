using System.Runtime.InteropServices;

namespace PinQuick.App.Services;

/// <summary>
/// Ana pencereye Win32 mesaj yakalama gömerek sistem tepsisi ikonu ve
/// global kısayol (Ctrl+Space) desteği sağlar.
/// Pencere gizlendiğinde mesajlar çalışmaya devam ettiği için tepsi ikonu
/// üzerinden yeniden gösterim mümkündür.
/// </summary>
internal sealed class NativeWindowBridge : IDisposable
{
    private const uint WmApp = 0x8000;
    private const uint WmHotkey = 0x0312;
    private const int WmRButtonUp = 0x0205;
    private const int WmLButtonDblClk = 0x0203;

    private const int NifMessage = 0x0001;
    private const int NifIcon = 0x0002;
    private const int NifTip = 0x0004;
    private const int NimAdd = 0x0000;
    private const int NimDelete = 0x0002;

    private const uint TpmReturnCmd = 0x0100;
    private const uint MfString = 0x0000;
    private const uint ImageIcon = 1;
    private const uint LrLoadFromFile = 0x00000010;

    private const int IdHotkey = 0x0A01;
    private const int CmdShow = 0x1001;
    private const int CmdExit = 0x1002;
    private const int IconSize = 32;
    private const uint IconSpriteIdEmpty = 0;

    private readonly nint _hwnd;
    private readonly nint _icon;
    private nint _menu;
    private bool _trayShown;
    private bool _hotkeyRegistered;
    private bool _disposed;

    private static SubclassProcDelegate? _subclassProc;

    public event Action? ToggleWindowRequested;

    public event Action? ExitRequested;

    public NativeWindowBridge(nint hwnd)
    {
        _hwnd = hwnd;
        _subclassProc ??= WndProc;
        SetWindowSubclass(hwnd, _subclassProc, 1, 0);
        _icon = LoadAppIcon();
    }

    /// <summary>
    /// Tepsi ikonu ve global kısayolu kullanıcı ayarlarına göre açıp kapatır.
    /// </summary>
    public void ApplySettings(bool minimizeToTray, bool hotkeyEnabled)
    {
        if (_disposed)
        {
            return;
        }

        if (minimizeToTray && !_trayShown)
        {
            AddTrayIcon();
            _trayShown = true;
        }
        else if (!minimizeToTray && _trayShown)
        {
            RemoveTrayIcon();
            _trayShown = false;
        }

        if (hotkeyEnabled && !_hotkeyRegistered)
        {
            RegisterHotKey(_hwnd, IdHotkey, ModControl | ModNoRepeat, VkSpace);
            _hotkeyRegistered = true;
        }
        else if (!hotkeyEnabled && _hotkeyRegistered)
        {
            UnregisterHotKey(_hwnd, IdHotkey);
            _hotkeyRegistered = false;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_trayShown)
        {
            RemoveTrayIcon();
        }

        if (_hotkeyRegistered)
        {
            UnregisterHotKey(_hwnd, IdHotkey);
            _hotkeyRegistered = false;
        }

        if (_menu != 0)
        {
            DestroyMenu(_menu);
            _menu = 0;
        }

        if (_icon != 0)
        {
            DestroyIcon(_icon);
        }
    }

    private void AddTrayIcon()
    {
        var data = new NotifyIconData
        {
            cbSize = Marshal.SizeOf<NotifyIconData>(),
            hWnd = _hwnd,
            uID = 1,
            uFlags = NifMessage | NifIcon | NifTip,
            uCallbackMessage = (int)WmApp,
            hIcon = _icon,
            szTip = Loc.T("TrayTooltip"),
        };
        Shell_NotifyIconW(NimAdd, ref data);
    }

    private void RemoveTrayIcon()
    {
        var data = new NotifyIconData
        {
            cbSize = Marshal.SizeOf<NotifyIconData>(),
            hWnd = _hwnd,
            uID = 1,
        };
        Shell_NotifyIconW(NimDelete, ref data);
    }

    private nint WndProc(nint hWnd, uint uMsg, nint wParam, nint lParam, nint uIdSubclass, nint dwRefData)
    {
        if (uMsg == WmHotkey && wParam == IdHotkey)
        {
            ToggleWindowRequested?.Invoke();
            return 0;
        }

        if (uMsg == WmApp)
        {
            var message = lParam.ToInt32();
            if (message == WmRButtonUp)
            {
                ShowContextMenu();
                return 0;
            }

            if (message == WmLButtonDblClk)
            {
                ToggleWindowRequested?.Invoke();
                return 0;
            }
        }

        return DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }

    private void ShowContextMenu()
    {
        if (_menu != 0)
        {
            DestroyMenu(_menu);
            _menu = 0;
        }

        _menu = CreatePopupMenu();
        AppendMenuW(_menu, MfString, CmdShow, Loc.T("TrayShow"));
        AppendMenuW(_menu, MfString, CmdExit, Loc.T("TrayExit"));

        GetCursorPos(out var point);
        SetForegroundWindow(_hwnd);

        var command = TrackPopupMenu(_menu, TpmReturnCmd, point.X, point.Y, 0, _hwnd, 0);
        if (command == CmdShow)
        {
            ToggleWindowRequested?.Invoke();
        }
        else if (command == CmdExit)
        {
            ExitRequested?.Invoke();
        }
    }

    private static nint LoadAppIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
        if (File.Exists(iconPath))
        {
            var handle = LoadImageW(IntPtr.Zero, iconPath, ImageIcon, IconSize, IconSize, LrLoadFromFile);
            if (handle != 0)
            {
                return handle;
            }
        }

        return LoadIconW(IntPtr.Zero, (nint)IconSpriteIdEmpty);
    }

    private const int ModControl = 0x0002;
    private const int ModNoRepeat = 0x4000;
    private const int VkSpace = 0x20;

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NotifyIconData
    {
        public int cbSize;
        public nint hWnd;
        public int uID;
        public int uFlags;
        public int uCallbackMessage;
        public nint hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public int dwState;
        public int dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public uint uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public int dwInfoFlags;
        public nuint guidItem;
        public nint hBalloonIcon;
    }

    private delegate nint SubclassProcDelegate(nint hWnd, uint uMsg, nint wParam, nint lParam, nint uIdSubclass, nint dwRefData);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool SetWindowSubclass(nint hWnd, SubclassProcDelegate pfnSubclass, nint uIdSubclass, nint dwRefData);

    [DllImport("comctl32.dll")]
    private static extern nint DefSubclassProc(nint hWnd, uint uMsg, nint wParam, nint lParam);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIconW(int dwMessage, ref NotifyIconData lpData);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint LoadImageW(nint hInst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint LoadIconW(nint hInstance, nint lpIconName);

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(nint hIcon);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(nint hWnd, int id);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool AppendMenuW(nint hMenu, uint uFlags, nint uIDNewItem, string lpNewItem);

    [DllImport("user32.dll")]
    private static extern nint TrackPopupMenu(nint hMenu, uint uFlags, int x, int y, int nReserved, nint hWnd, nint prcRect);

    [DllImport("user32.dll")]
    private static extern bool DestroyMenu(nint hMenu);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out NativePoint lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(nint hWnd);
}