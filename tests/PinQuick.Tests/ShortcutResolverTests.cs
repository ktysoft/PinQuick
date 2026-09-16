using System.Runtime.InteropServices;
using PinQuick.Windows;

#pragma warning disable CA1416 // Kısayol oluşturma testleri yalnızca Windows'ta çalışır.

namespace PinQuick.Tests;

public sealed class ShortcutResolverTests
{
    [Fact]
    public void IsShortcut_ForLnkAndUrl_ReturnsTrue_OthersFalse()
    {
        Assert.True(ShortcutResolver.IsShortcut(@"C:\test\App.lnk"));
        Assert.True(ShortcutResolver.IsShortcut(@"C:\test\site.url"));
        Assert.True(ShortcutResolver.IsShortcut(@"C:\test\App.LNK"));
        Assert.False(ShortcutResolver.IsShortcut(@"C:\test\app.exe"));
        Assert.False(ShortcutResolver.IsShortcut(null));
    }

    [Fact]
    public void ResolveIconSource_NonShortcut_ReturnsNull()
    {
        var exePath = Path.Combine(Environment.SystemDirectory, "cmd.exe");
        Assert.True(File.Exists(exePath));
        Assert.Null(ShortcutResolver.ResolveIconSource(exePath));
    }

    [Fact]
    public void ResolveIconSource_MissingShortcut_ReturnsNull()
    {
        var missing = Path.Combine(Path.GetTempPath(), $"pinquick-missing-{Guid.NewGuid():N}.lnk");
        Assert.Null(ShortcutResolver.ResolveIconSource(missing));
    }

    [Fact]
    public void ResolveLnk_ToExecutable_ReturnsTargetPath()
    {
        var target = Path.Combine(Environment.SystemDirectory, "cmd.exe");
        var lnkPath = Path.Combine(Path.GetTempPath(), $"pinquick-{Guid.NewGuid():N}.lnk");
        try
        {
            CreateShortcut(lnkPath, target, iconLocation: null);
            Assert.Equal(target, ShortcutResolver.ResolveIconSource(lnkPath), ignoreCase: true);
        }
        finally
        {
            File.Delete(lnkPath);
        }
    }

    [Fact]
    public void ResolveLnk_ToFolder_ReturnsFolderPath()
    {
        var folder = Directory.CreateTempSubdirectory("pinquick-shortcut-");
        var lnkPath = Path.Combine(Path.GetTempPath(), $"pinquick-{Guid.NewGuid():N}.lnk");
        try
        {
            CreateShortcut(lnkPath, folder.FullName, iconLocation: null);
            Assert.Equal(folder.FullName, ShortcutResolver.ResolveIconSource(lnkPath), ignoreCase: true);
        }
        finally
        {
            File.Delete(lnkPath);
            try
            {
                Directory.Delete(folder.FullName, recursive: true);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public void ResolveLnk_WithCustomIcon_ReturnsIconPath()
    {
        var target = Path.Combine(Environment.SystemDirectory, "cmd.exe");
        var iconPath = Path.Combine(Environment.SystemDirectory, "shell32.dll");
        var lnkPath = Path.Combine(Path.GetTempPath(), $"pinquick-{Guid.NewGuid():N}.lnk");
        try
        {
            CreateShortcut(lnkPath, target, iconPath);
            Assert.Equal(iconPath, ShortcutResolver.ResolveIconSource(lnkPath), ignoreCase: true);
        }
        finally
        {
            File.Delete(lnkPath);
        }
    }

    [Fact]
    public void ResolveUrl_WithIconFile_ReturnsIconPath()
    {
        var iconPath = Path.Combine(Environment.SystemDirectory, "shell32.dll");
        var urlPath = Path.Combine(Path.GetTempPath(), $"pinquick-{Guid.NewGuid():N}.url");
        try
        {
            File.WriteAllText(urlPath,
                "[InternetShortcut]\r\nURL=https://example.com\r\nIconFile=" + iconPath + "\r\nIconIndex=0\r\n");
            Assert.Equal(iconPath, ShortcutResolver.ResolveIconSource(urlPath), ignoreCase: true);
        }
        finally
        {
            File.Delete(urlPath);
        }
    }

    [Fact]
    public void ResolveUrl_WithoutIcon_ReturnsNull()
    {
        var urlPath = Path.Combine(Path.GetTempPath(), $"pinquick-{Guid.NewGuid():N}.url");
        try
        {
            File.WriteAllText(urlPath, "[InternetShortcut]\r\nURL=https://example.com\r\n");
            Assert.Null(ShortcutResolver.ResolveIconSource(urlPath));
        }
        finally
        {
            File.Delete(urlPath);
        }
    }

    [Fact]
    public void ResolveShortcut_NonShortcut_ReturnsNull()
    {
        var exePath = Path.Combine(Environment.SystemDirectory, "cmd.exe");
        Assert.True(File.Exists(exePath));
        Assert.Null(ShortcutResolver.ResolveShortcut(exePath));
    }

    [Fact]
    public void ResolveShortcut_MissingShortcut_ReturnsNull()
    {
        var missing = Path.Combine(Path.GetTempPath(), $"pinquick-missing-{Guid.NewGuid():N}.lnk");
        Assert.Null(ShortcutResolver.ResolveShortcut(missing));
    }

    [Fact]
    public void ResolveShortcut_LnkToExecutable_ReturnsTargetPath()
    {
        var target = Path.Combine(Environment.SystemDirectory, "cmd.exe");
        var lnkPath = Path.Combine(Path.GetTempPath(), $"pinquick-{Guid.NewGuid():N}.lnk");
        try
        {
            CreateShortcut(lnkPath, target, iconLocation: null);
            var shortcut = ShortcutResolver.ResolveShortcut(lnkPath);
            Assert.NotNull(shortcut);
            Assert.Equal(target, shortcut.Target, ignoreCase: true);
            Assert.Equal(string.Empty, shortcut.Arguments);
        }
        finally
        {
            File.Delete(lnkPath);
        }
    }

    [Fact]
    public void ResolveShortcut_LnkWithArguments_ReturnsArguments()
    {
        var target = Path.Combine(Environment.SystemDirectory, "cmd.exe");
        var lnkPath = Path.Combine(Path.GetTempPath(), $"pinquick-{Guid.NewGuid():N}.lnk");
        const string arguments = "/k ver";
        try
        {
            CreateShortcut(lnkPath, target, iconLocation: null, arguments);
            var shortcut = ShortcutResolver.ResolveShortcut(lnkPath);
            Assert.NotNull(shortcut);
            Assert.Equal(target, shortcut.Target, ignoreCase: true);
            Assert.Equal(arguments, shortcut.Arguments);
        }
        finally
        {
            File.Delete(lnkPath);
        }
    }

    [Fact]
    public void ResolveShortcut_LnkToFolder_ReturnsFolderTarget()
    {
        var folder = Directory.CreateTempSubdirectory("pinquick-shortcut-");
        var lnkPath = Path.Combine(Path.GetTempPath(), $"pinquick-{Guid.NewGuid():N}.lnk");
        try
        {
            CreateShortcut(lnkPath, folder.FullName, iconLocation: null);
            var shortcut = ShortcutResolver.ResolveShortcut(lnkPath);
            Assert.NotNull(shortcut);
            Assert.Equal(folder.FullName, shortcut.Target, ignoreCase: true);
        }
        finally
        {
            File.Delete(lnkPath);
            try
            {
                Directory.Delete(folder.FullName, recursive: true);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public void ResolveShortcut_Url_ReturnsUrlTarget()
    {
        var urlPath = Path.Combine(Path.GetTempPath(), $"pinquick-{Guid.NewGuid():N}.url");
        try
        {
            File.WriteAllText(urlPath, "[InternetShortcut]\r\nURL=https://example.com\r\n");
            var shortcut = ShortcutResolver.ResolveShortcut(urlPath);
            Assert.NotNull(shortcut);
            Assert.Equal("https://example.com", shortcut.Target);
            Assert.Equal(string.Empty, shortcut.Arguments);
        }
        finally
        {
            File.Delete(urlPath);
        }
    }

    [Fact]
    public void ResolveShortcut_UrlToLocalFile_ReturnsLocalPath()
    {
        var target = Path.Combine(Path.GetTempPath(), $"pinquick-app-{Guid.NewGuid():N}.exe");
        File.WriteAllText(target, string.Empty);
        var urlPath = Path.Combine(Path.GetTempPath(), $"pinquick-{Guid.NewGuid():N}.url");
        try
        {
            File.WriteAllText(urlPath, $"[InternetShortcut]\r\nURL={new Uri(target).AbsoluteUri}\r\n");
            var shortcut = ShortcutResolver.ResolveShortcut(urlPath);
            Assert.NotNull(shortcut);
            Assert.Equal(target, shortcut.Target, ignoreCase: true);
            Assert.Equal(string.Empty, shortcut.Arguments);
        }
        finally
        {
            File.Delete(urlPath);
            File.Delete(target);
        }
    }

    [Fact]
    public void IsAppLauncherScheme_CommonLaunchers_ReturnsTrue()
    {
        Assert.True(ShortcutResolver.IsAppLauncherScheme("steam"));
        Assert.True(ShortcutResolver.IsAppLauncherScheme("com.epicgames.launcher"));
        Assert.True(ShortcutResolver.IsAppLauncherScheme("battlenet"));
        Assert.True(ShortcutResolver.IsAppLauncherScheme("xboxlauncher"));
        Assert.True(ShortcutResolver.IsAppLauncherScheme("origin"));
        Assert.True(ShortcutResolver.IsAppLauncherScheme("uplay"));
    }

    [Fact]
    public void IsAppLauncherScheme_WebAndOtherSchemes_ReturnsFalse()
    {
        Assert.False(ShortcutResolver.IsAppLauncherScheme("https"));
        Assert.False(ShortcutResolver.IsAppLauncherScheme("http"));
        Assert.False(ShortcutResolver.IsAppLauncherScheme("mailto"));
        Assert.False(ShortcutResolver.IsAppLauncherScheme("file"));
        Assert.False(ShortcutResolver.IsAppLauncherScheme(""));
    }

    private static void CreateShortcut(string lnkPath, string targetPath, string? iconLocation, string? arguments = null)
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("WScript.Shell bulunamadı.");

        dynamic shell = Activator.CreateInstance(shellType)
            ?? throw new InvalidOperationException("WScript.Shell oluşturulamadı.");
        dynamic shortcut = shell.CreateShortcut(lnkPath);
        shortcut.TargetPath = targetPath;
        if (iconLocation is not null)
        {
            shortcut.IconLocation = $"{iconLocation},0";
        }

        if (arguments is not null)
        {
            shortcut.Arguments = arguments;
        }

        shortcut.Save();
        Marshal.FinalReleaseComObject((object)shortcut);
        Marshal.FinalReleaseComObject((object)shell);
    }
}