using System.Runtime.InteropServices;
using System.Text;
using PinQuick.Core.Security;

#pragma warning disable CA1416 // Bu sınıf yalnızca Windows kısayolları (COM/shell) için Windows'ta çalışır.

namespace PinQuick.Windows;

/// <summary>
/// Windows kısayollarını (.lnk / .url) ikonun gerçek kaynağına çözer.
/// Kısayol dosyasının kendisi yerine hedef dosya/klasörün yolu döndürülür;
/// böylece ikon, kısayol dosyasından değil gerçek hedeften alınır.
/// </summary>
public static class ShortcutResolver
{
    private const int MaxPath = 260;

    /// <summary>
    /// Yol bir Windows kısayolu (.lnk veya .url) ise <c>true</c> döner.
    /// </summary>
    public static bool IsShortcut(string? path)
        => !string.IsNullOrWhiteSpace(path)
           && (path!.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase)
               || path.EndsWith(".url", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Kısayolun ikonunu temsil eden gerçek dosya/klasör yolunu döndürür.
    /// Kısayol değilse, dosya mevcut değilse veya çözümlenemiyorsa <c>null</c> döner.
    /// </summary>
    public static string? ResolveIconSource(string path)
    {
        if (!IsShortcut(path))
        {
            return null;
        }

        string fullPath;
        try
        {
            fullPath = PathValidation.ResolveEnvironmentVariables(path);
        }
        catch (ArgumentException)
        {
            return null;
        }

        if (!File.Exists(fullPath))
        {
            return null;
        }

        try
        {
            return fullPath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase)
                ? ResolveLnk(fullPath)
                : ResolveUrl(fullPath);
        }
        catch
        {
            return null;
        }
    }

    private static string? ResolveLnk(string lnkPath)
    {
        var shellLink = (IShellLinkW)(object)new ShellLink();
        try
        {
            ((IPersistFile)shellLink).Load(lnkPath, StgmRead);

            var iconPath = new StringBuilder(MaxPath);
            if (shellLink.GetIconLocation(iconPath, iconPath.Capacity, out _) == S_OK)
            {
                var customIcon = iconPath.ToString().Trim();
                if (customIcon.Length > 0)
                {
                    var expanded = Environment.ExpandEnvironmentVariables(customIcon);
                    if (File.Exists(expanded))
                    {
                        return expanded;
                    }
                }
            }

            var targetPath = new StringBuilder(MaxPath);
            if (shellLink.GetPath(targetPath, targetPath.Capacity, IntPtr.Zero, SlgpRawPath) == S_OK)
            {
                var target = Environment.ExpandEnvironmentVariables(targetPath.ToString());
                if (File.Exists(target) || Directory.Exists(target))
                {
                    return target;
                }
            }

            return null;
        }
        finally
        {
            Marshal.ReleaseComObject(shellLink);
        }
    }

    private static string? ResolveUrl(string urlPath)
    {
        var iconPath = new StringBuilder(MaxPath);
        if (GetPrivateProfileStringW("InternetShortcut", "IconFile", null, iconPath, iconPath.Capacity, urlPath) == 0)
        {
            return null;
        }

        var expanded = Environment.ExpandEnvironmentVariables(iconPath.ToString().Trim());
        return File.Exists(expanded) ? expanded : null;
    }

    private const int S_OK = 0;
    private const uint StgmRead = 0;
    private const uint SlgpRawPath = 0x00000004;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetPrivateProfileStringW(
        string lpAppName,
        string? lpKeyName,
        string? lpDefault,
        StringBuilder lpReturnedString,
        int nSize,
        string lpFileName);

    [ComImport, Guid("00021401-0000-0000-C000-000000000046")]
    private sealed class ShellLink
    {
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000010B-0000-0000-C000-000000000046")]
    private interface IPersistFile
    {
        [PreserveSig]
        int GetClassID(out Guid pClassID);

        [PreserveSig]
        int IsDirty();

        [PreserveSig]
        int Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);

        [PreserveSig]
        int Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, bool fRemember);

        [PreserveSig]
        int SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

        [PreserveSig]
        int GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLinkW
    {
        [PreserveSig]
        int GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, uint fFlags);

        [PreserveSig]
        int GetIDList(out IntPtr ppidl);

        [PreserveSig]
        int SetIDList(IntPtr pidl);

        [PreserveSig]
        int GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);

        [PreserveSig]
        int SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

        [PreserveSig]
        int GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);

        [PreserveSig]
        int SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

        [PreserveSig]
        int GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);

        [PreserveSig]
        int SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

        [PreserveSig]
        int GetHotkey(out ushort pwHotkey);

        [PreserveSig]
        int SetHotkey(ushort wHotkey);

        [PreserveSig]
        int GetShowCmd(out int piShowCmd);

        [PreserveSig]
        int SetShowCmd(int iShowCmd);

        [PreserveSig]
        int GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);

        [PreserveSig]
        int SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

        [PreserveSig]
        int SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);

        [PreserveSig]
        int Resolve(IntPtr hwnd, uint fFlags);

        [PreserveSig]
        int SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }
}