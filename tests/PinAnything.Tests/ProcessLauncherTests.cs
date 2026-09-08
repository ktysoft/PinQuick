using PinAnything.Core.Models;
using PinAnything.Windows;

namespace PinAnything.Tests;

public sealed class ProcessLauncherTests
{
    private readonly ProcessLauncher _launcher = new();

    [Fact]
    public void BuildStartInfo_Application_UsesShellExecute()
    {
        var pin = new Pin
        {
            Title = "Notepad",
            Type = PinType.Application,
            Target = "notepad.exe",
            Arguments = "C:\\temp\\file.txt",
        };

        var info = _launcher.BuildStartInfo(pin);

        Assert.NotNull(info);
        Assert.Equal("notepad.exe", info!.FileName);
        Assert.True(info.UseShellExecute);
        Assert.Equal("C:\\temp\\file.txt", info.Arguments);
    }

    [Fact]
    public void BuildStartInfo_Administrator_UsesRunAsVerb()
    {
        var pin = new Pin
        {
            Title = "CMD admin",
            Type = PinType.Command,
            Target = "ipconfig /all",
            RunAsAdministrator = true,
        };

        var info = _launcher.BuildStartInfo(pin);

        Assert.NotNull(info);
        Assert.Equal("runas", info!.Verb);
    }

    [Fact]
    public void BuildStartInfo_Website_UsesUrl()
    {
        var pin = new Pin
        {
            Title = "GitHub",
            Type = PinType.Website,
            Target = "https://github.com",
        };

        var info = _launcher.BuildStartInfo(pin);

        Assert.NotNull(info);
        Assert.Equal("https://github.com", info!.FileName);
        Assert.True(info.UseShellExecute);
    }

    [Fact]
    public void BuildStartInfo_Website_InvalidUrl_ReturnsNull()
    {
        var pin = new Pin
        {
            Title = "Boş",
            Type = PinType.Website,
            Target = "github.com",
        };

        Assert.Null(_launcher.BuildStartInfo(pin));
    }

    [Fact]
    public void BuildStartInfo_WindowsSetting_UsesUri()
    {
        var pin = new Pin
        {
            Title = "Ağ Ayarları",
            Type = PinType.WindowsSetting,
            Target = "ms-settings:network",
        };

        var info = _launcher.BuildStartInfo(pin);

        Assert.NotNull(info);
        Assert.Equal("ms-settings:network", info!.FileName);
        Assert.True(info.UseShellExecute);
    }

    [Fact]
    public void BuildStartInfo_Command_UsesCmdWithArgumentList()
    {
        var pin = new Pin
        {
            Title = "ipconfig",
            Type = PinType.Command,
            Target = "ipconfig /all",
        };

        var info = _launcher.BuildStartInfo(pin);

        Assert.NotNull(info);
        Assert.Equal("cmd.exe", info!.FileName);
        Assert.False(info.UseShellExecute);
        Assert.Contains(info.ArgumentList, argument => argument.Contains("ipconfig /all", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildStartInfo_PowerShell_UsesPowershell()
    {
        var pin = new Pin
        {
            Title = "Get-Process",
            Type = PinType.PowerShell,
            Target = "Get-Process",
        };

        var info = _launcher.BuildStartInfo(pin);

        Assert.NotNull(info);
        Assert.Equal("powershell.exe", info!.FileName);
    }

    [Fact]
    public void BuildStartInfo_EmptyTarget_ReturnsNull()
    {
        var pin = new Pin
        {
            Title = "Boş",
            Type = PinType.Application,
            Target = "   ",
        };

        Assert.Null(_launcher.BuildStartInfo(pin));
    }

    [Fact]
    public void BuildStartInfo_Custom_WithEmptyTarget_ReturnsNull()
    {
        var pin = new Pin
        {
            Title = "Boş özel",
            Type = PinType.Custom,
            Target = "",
        };

        Assert.Null(_launcher.BuildStartInfo(pin));
    }

    [Fact]
    public void BuildStartInfo_Folder_UsesExplorer()
    {
        var pin = new Pin
        {
            Title = "Projeler",
            Type = PinType.Folder,
            Target = @"C:\Projects",
        };

        var info = _launcher.BuildStartInfo(pin);

        Assert.NotNull(info);
        Assert.Equal("explorer.exe", info!.FileName);
        Assert.True(info.UseShellExecute);
    }

    [Fact]
    public void BuildStartInfo_DisabledPin_StartReturnsFailed()
    {
        var pin = new Pin
        {
            Title = "Kapalı",
            Type = PinType.Custom,
            Target = @"C:\nothing.exe",
            IsEnabled = false,
        };

        var result = _launcher.Start(pin);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void BuildStartInfo_SystemTool_ResolvesSystemPath()
    {
        var pin = new Pin
        {
            Title = "Aygıt Yöneticisi",
            Type = PinType.SystemTool,
            Target = "devmgmt.msc",
        };

        var info = _launcher.BuildStartInfo(pin);

        Assert.NotNull(info);
        Assert.EndsWith("devmgmt.msc", info!.FileName, StringComparison.OrdinalIgnoreCase);
        Assert.True(info.UseShellExecute);
    }
}