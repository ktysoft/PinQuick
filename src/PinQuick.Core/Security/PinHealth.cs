using PinQuick.Core.Models;

namespace PinQuick.Core.Security;

/// <summary>
/// Bir pinin hedefinin şu an erişilebilir/geçerli olup olmadığını değerlendirir.
/// Bu kontrol "bozuk pin" tespitinde kullanılır.
/// </summary>
public static class PinHealth
{
    /// <summary>
    /// Pin hedefi erişilemez veya biçimsel olarak geçersizse <c>true</c> döner.
    /// Ağ yolları ve komut tarzı hedefler geçici erişilemezlikten etkilenmemesi için
    /// bozuk sayılmazlar.
    /// </summary>
    public static bool IsBroken(Pin pin)
    {
        ArgumentNullException.ThrowIfNull(pin);

        if (string.IsNullOrWhiteSpace(pin.Target))
        {
            return false;
        }

        switch (pin.Type)
        {
            case PinType.Application:
            case PinType.File:
            case PinType.Batch:
                return !File.Exists(PathValidation.ResolveEnvironmentVariables(pin.Target));

            case PinType.Folder:
                return !Directory.Exists(PathValidation.ResolveEnvironmentVariables(pin.Target));

            case PinType.PowerShell:
                return IsScriptFile(pin.Target)
                    && !File.Exists(PathValidation.ResolveEnvironmentVariables(pin.Target));

            case PinType.SystemTool:
                return !SystemToolExists(pin.Target);

            case PinType.Website:
            case PinType.Url:
                return !PathValidation.IsValidUrl(pin.Target);

            case PinType.WindowsSetting:
                return !PathValidation.IsValidWindowsUri(pin.Target);

            default:
                // Command, NetworkPath ve Custom: her zaman erişilebilir kabul edilir.
                return false;
        }
    }

    private static bool IsScriptFile(string target)
        => target.Trim().EndsWith(".ps1", StringComparison.OrdinalIgnoreCase);

    private static bool SystemToolExists(string target)
    {
        var resolved = PathValidation.ResolveEnvironmentVariables(target);

        if (resolved.EndsWith(".msc", StringComparison.OrdinalIgnoreCase) && !File.Exists(resolved))
        {
            resolved = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), resolved);
        }

        return File.Exists(resolved);
    }
}