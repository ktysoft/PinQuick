using PinAnything.Core.Models;

namespace PinAnything.Core.Security;

/// <summary>
/// Pin hedeflerinin türüne göre doğrulanmasını sağlar.
/// </summary>
public static partial class PathValidation
{
    /// <summary>
    /// Hedefin pin türüne göre temel geçerlilik kontrolünü yapar.
    /// Bazı türlerde (network, website) hedefe erişilememek pinin geçersiz olduğu anlamına gelmez.
    /// </summary>
    public static bool IsTargetRequired(PinType type)
        => type is not PinType.Custom;

    public static bool IsValidTarget(PinType type, string target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return false;
        }

        return type switch
        {
            PinType.Website or PinType.Url => IsValidUrl(target),
            PinType.WindowsSetting => IsValidWindowsUri(target),
            PinType.NetworkPath => target.StartsWith(@"\\", StringComparison.Ordinal) || target.StartsWith(@"//", StringComparison.Ordinal),
            PinType.Application or PinType.File => File.Exists(ResolveEnvironmentVariables(target)),
            PinType.Folder => Directory.Exists(ResolveEnvironmentVariables(target)),
            PinType.Command or PinType.PowerShell or PinType.Batch or PinType.SystemTool or PinType.Custom => true,
            _ => true,
        };
    }

    public static bool IsValidUrl(string target)
    {
        if (!Uri.TryCreate(target, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }

    public static bool IsValidWindowsUri(string target)
    {
        if (!Uri.TryCreate(target, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Scheme == "ms-settings";
    }

    /// <summary>
    /// %USERPROFILE% gibi ortam değişkenlerini tam yola dönüştürür.
    /// </summary>
    public static string ResolveEnvironmentVariables(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Environment.ExpandEnvironmentVariables(path);
    }
}