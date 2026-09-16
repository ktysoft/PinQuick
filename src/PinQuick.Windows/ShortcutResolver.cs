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
    private const int MaxShortcutDepth = 5;

    /// <summary>
    /// Bir Windows kısayolunun çözümlenmiş gerçek hedefi. <see cref="Target"/> bir
    /// .lnk için gerçek dosya/klasör yolu, .url için URL değeridir. <see cref="Arguments"/>,
    /// .lnk kısayolunda saklanan komut satırı argümanlarıdır (.url için boş olur).
    /// </summary>
    public sealed record ShortcutInfo(string Target, string Arguments = "");

    /// <summary>
    /// Yol bir Windows kısayolu (.lnk veya .url) ise <c>true</c> döner.
    /// </summary>
    public static bool IsShortcut(string? path)
        => !string.IsNullOrWhiteSpace(path)
           && (path!.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase)
               || path.EndsWith(".url", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Kısayolun gerçek hedefini döndürür: .lnk için hedef dosya/klasör yolu,
    /// .url için URL. Kısayol değilse, hedef mevcut değilse veya çözümlenemiyorsa
    /// <c>null</c> döner.
    /// </summary>
    public static ShortcutInfo? ResolveShortcut(string path)
    {
        if (!IsShortcut(path))
        {
            return null;
        }

        return ResolveShortcutCore(path);
    }

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
                : ResolveUrlIcon(fullPath);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Bir URI şemasının bilinen bir masaüstü uygulama başlatıcısına ait olup
    /// olmadığını döndürür. steam://, com.epicgames.launcher:// gibi hedefler
    /// bir web sitesi değil, yüklü bir masaüstü uygulamasını başlatır.
    /// </summary>
    public static bool IsAppLauncherScheme(string scheme)
    {
        return scheme.Equals("steam", StringComparison.OrdinalIgnoreCase)
            || scheme.Equals("com.epicgames.launcher", StringComparison.OrdinalIgnoreCase)
            || scheme.Equals("battlenet", StringComparison.OrdinalIgnoreCase)
            || scheme.Equals("xboxlauncher", StringComparison.OrdinalIgnoreCase)
            || scheme.Equals("origin", StringComparison.OrdinalIgnoreCase)
            || scheme.Equals("uplay", StringComparison.OrdinalIgnoreCase);
    }

    private static ShortcutInfo? ResolveShortcutCore(string path)
    {
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
                ? ResolveLnkShortcut(fullPath)
                : ResolveUrlShortcut(fullPath);
        }
        catch
        {
            return null;
        }
    }

    private static ShortcutInfo? ResolveLnkShortcut(string lnkPath)
    {
        var shellLink = (IShellLinkW)(object)new ShellLink();
        try
        {
            ((IPersistFile)shellLink).Load(lnkPath, StgmRead);

            var target = GetTargetPath(shellLink);

            // Kısayolun hedefi başka bir kısayol olabilir (ör. zincirleme .lnk).
            for (var depth = 0; target is not null && depth < MaxShortcutDepth; depth++)
            {
                if (!IsShortcut(target))
                {
                    break;
                }

                var nested = ResolveShortcutCore(target);
                if (nested is null)
                {
                    return null;
                }

                target = nested.Target;
            }

            if (target is null)
            {
                return null;
            }

            var arguments = new StringBuilder(MaxPath);
            var args = shellLink.GetArguments(arguments, arguments.Capacity) == S_OK
                ? arguments.ToString().Trim()
                : string.Empty;

            return new ShortcutInfo(target, args);
        }
        finally
        {
            Marshal.ReleaseComObject(shellLink);
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

            return GetTargetPath(shellLink);
        }
        finally
        {
            Marshal.ReleaseComObject(shellLink);
        }
    }

    private static string? GetTargetPath(IShellLinkW shellLink)
    {
        var targetPath = new StringBuilder(MaxPath);
        if (shellLink.GetPath(targetPath, targetPath.Capacity, IntPtr.Zero, SlgpRawPath) != S_OK)
        {
            return null;
        }

        var target = Environment.ExpandEnvironmentVariables(targetPath.ToString()).Trim();
        return target.Length > 0 && (File.Exists(target) || Directory.Exists(target)) ? target : null;
    }

    private static ShortcutInfo? ResolveUrlShortcut(string urlPath)
    {
        var url = new StringBuilder(MaxPath);
        if (GetPrivateProfileStringW("InternetShortcut", "URL", null, url, url.Capacity, urlPath) == 0)
        {
            return null;
        }

        var target = url.ToString().Trim();
        if (string.IsNullOrEmpty(target))
        {
            return null;
        }

        // file:/// ve file://localhost/ URI'lerini yerel dosya yoluna dönüştür;
        // böylece uygulama vb. .url kısayolları doğru hedefe çözümlenir.
        if (Uri.TryCreate(target, UriKind.Absolute, out var uri)
            && uri.IsFile
            && Uri.IsWellFormedUriString(target, UriKind.Absolute))
        {
            var localPath = uri.LocalPath;
            if (File.Exists(localPath) || Directory.Exists(localPath))
            {
                return new ShortcutInfo(localPath, string.Empty);
            }
        }

        return new ShortcutInfo(target, string.Empty);
    }

    private static string? ResolveUrlIcon(string urlPath)
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