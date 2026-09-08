using System.Diagnostics;
using PinQuick.Core.Models;

namespace PinQuick.Windows;

/// <summary>
/// Pin hedeflerini Windows mekanizmalarıyla başlatır (ShellExecute, URL, komut, PowerShell).
/// UAC atlatmaz; yönetici gerektiğinde standart "runas" elevation kullanır.
/// </summary>
public sealed class ProcessLauncher
{
    /// <summary>
    /// Pini başlatır. Başlatılamazsa LaunchResult hata bilgisiyle döner; exception fırlatmaz.
    /// </summary>
    public LaunchResult Start(Pin pin)
    {
        ArgumentNullException.ThrowIfNull(pin);

        if (!pin.IsEnabled)
        {
            return LaunchResult.Failed("Bu pin devre dışı.");
        }

        var startInfo = BuildStartInfo(pin);
        if (startInfo is null)
        {
            return LaunchResult.Failed("Bu pin türü için çalıştırma bilgisi oluşturulamadı.");
        }

        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return LaunchResult.Failed("Sistem çalıştırmayı başlatamadı.");
            }

            return LaunchResult.ProcessStarted(process.Id);
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception
                                       or System.IO.IOException
                                       or InvalidOperationException)
        {
            return LaunchResult.Failed($"Pin başlatılamadı: {ex.Message}");
        }
    }

    /// <summary>
    /// Pin türüne göre ProcessStartInfo üretir. Geçersiz hedeflerde null döner.
    /// </summary>
    public ProcessStartInfo? BuildStartInfo(Pin pin)
    {
        if (string.IsNullOrWhiteSpace(pin.Target))
        {
            return null;
        }

        var target = Core.Security.PathValidation.ResolveEnvironmentVariables(pin.Target);

        if (string.IsNullOrWhiteSpace(target))
        {
            return null;
        }

        var startInfo = pin.Type switch
        {
            PinType.Application or PinType.File => CreateApplicationStartInfo(pin, target),
            PinType.Folder or PinType.NetworkPath => CreateExplorerStartInfo(pin, target),
            PinType.Website or PinType.Url => CreateUrlStartInfo(pin, target),
            PinType.WindowsSetting => CreateUriStartInfo(target),
            PinType.SystemTool => CreateSystemToolStartInfo(pin, target),
            PinType.Command => CreateCmdStartInfo(pin, target),
            PinType.PowerShell => CreatePowerShellStartInfo(pin, target),
            PinType.Batch => CreateBatchStartInfo(pin, target),
            PinType.Custom => CreateCustomStartInfo(pin),
            _ => null,
        };

        if (startInfo is null)
        {
            return null;
        }

        ApplyCommonOptions(startInfo, pin);
        return startInfo;
    }

    private static ProcessStartInfo CreateApplicationStartInfo(Pin pin, string target)
    {
        var startInfo = new ProcessStartInfo(target)
        {
            UseShellExecute = true,
            Verb = GetVerb(pin),
        };

        if (!string.IsNullOrWhiteSpace(pin.Arguments))
        {
            startInfo.Arguments = pin.Arguments;
        }

        return startInfo;
    }

    private static ProcessStartInfo CreateExplorerStartInfo(Pin pin, string target)
    {
        var startInfo = new ProcessStartInfo("explorer.exe")
        {
            UseShellExecute = true,
        };
        startInfo.ArgumentList.Add($"\"{target}\"");
        return startInfo;
    }

    private static ProcessStartInfo? CreateUrlStartInfo(Pin pin, string target)
    {
        if (!Core.Security.PathValidation.IsValidUrl(target))
        {
            return null;
        }

        return new ProcessStartInfo(target)
        {
            UseShellExecute = true,
            Verb = GetVerb(pin),
        };
    }

    private static ProcessStartInfo? CreateUriStartInfo(string target)
    {
        if (!Uri.TryCreate(target, UriKind.Absolute, out _))
        {
            return null;
        }

        return new ProcessStartInfo(target)
        {
            UseShellExecute = true,
        };
    }

    private static ProcessStartInfo? CreateSystemToolStartInfo(Pin pin, string target)
    {
        if (target.EndsWith(".msc", StringComparison.OrdinalIgnoreCase) && !File.Exists(target))
        {
            target = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), target);
        }

        if (!File.Exists(target))
        {
            return null;
        }

        return new ProcessStartInfo(target)
        {
            UseShellExecute = true,
            Verb = GetVerb(pin),
        };
    }

    private static ProcessStartInfo CreateCmdStartInfo(Pin pin, string target)
    {
        var combined = CombineCommand(target, pin.Arguments);

        if (pin.RunAsAdministrator)
        {
            return new ProcessStartInfo("cmd.exe")
            {
                UseShellExecute = true,
                Verb = "runas",
                Arguments = $"/d /c \"{combined}\"",
            };
        }

        var startInfo = new ProcessStartInfo("cmd.exe")
        {
            UseShellExecute = false,
            CreateNoWindow = false,
        };

        startInfo.ArgumentList.Add("/d");
        startInfo.ArgumentList.Add("/c");
        AppendCommand(startInfo, combined);
        return startInfo;
    }

    private static ProcessStartInfo CreatePowerShellStartInfo(Pin pin, string target)
    {
        if (pin.RunAsAdministrator)
        {
            var elevated = CombineCommand(target, pin.Arguments);
            return new ProcessStartInfo("powershell.exe")
            {
                UseShellExecute = true,
                Verb = "runas",
                Arguments = $"-NoExit -NoProfile -Command \"{elevated}\"",
            };
        }

        var startInfo = new ProcessStartInfo("powershell.exe")
        {
            UseShellExecute = false,
            CreateNoWindow = false,
        };

        startInfo.ArgumentList.Add("-NoExit");
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-Command");
        AppendCommand(startInfo, CombineCommand(target, pin.Arguments));
        return startInfo;
    }

    private static ProcessStartInfo? CreateBatchStartInfo(Pin pin, string target)
    {
        if (!File.Exists(target))
        {
            return null;
        }

        var combined = CombineCommand($"\"{target}\"", pin.Arguments);

        if (pin.RunAsAdministrator)
        {
            return new ProcessStartInfo("cmd.exe")
            {
                UseShellExecute = true,
                Verb = "runas",
                Arguments = $"/d /c {combined}",
            };
        }

        var startInfo = new ProcessStartInfo("cmd.exe")
        {
            UseShellExecute = false,
            CreateNoWindow = false,
        };

        startInfo.ArgumentList.Add("/d");
        startInfo.ArgumentList.Add("/c");
        AppendCommand(startInfo, combined);
        return startInfo;
    }

    private static ProcessStartInfo? CreateCustomStartInfo(Pin pin)
    {
        if (string.IsNullOrWhiteSpace(pin.Target))
        {
            return null;
        }

        return new ProcessStartInfo(pin.Target)
        {
            UseShellExecute = true,
            Verb = GetVerb(pin),
        };
    }

    private static void AppendCommand(ProcessStartInfo startInfo, string command)
    {
        startInfo.ArgumentList.Add($"\"{command}\"");
    }

    private static string CombineCommand(string command, string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return command;
        }

        return $"{command} {arguments}";
    }

    private static void ApplyCommonOptions(ProcessStartInfo startInfo, Pin pin)
    {
        if (!string.IsNullOrWhiteSpace(pin.WorkingDirectory)
            && Directory.Exists(Core.Security.PathValidation.ResolveEnvironmentVariables(pin.WorkingDirectory)))
        {
            startInfo.WorkingDirectory = Core.Security.PathValidation.ResolveEnvironmentVariables(pin.WorkingDirectory);
        }
    }

    private static string GetVerb(Pin pin)
        => pin.RunAsAdministrator ? "runas" : "open";
}

/// <summary>
/// Çalıştırma girişiminin sonucu.
/// </summary>
public sealed record LaunchResult
{
    private LaunchResult(bool success, int? processId, string? errorMessage)
    {
        Success = success;
        ProcessId = processId;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }

    public int? ProcessId { get; }

    public string? ErrorMessage { get; }

    public static LaunchResult ProcessStarted(int processId) => new(true, processId, null);

    public static LaunchResult Failed(string errorMessage) => new(false, null, errorMessage);
}