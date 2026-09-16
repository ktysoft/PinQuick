using System.Reflection;

namespace PinQuick.App;

/// <summary>
/// Uygulama kimliği ve sürüm bilgisi.
/// </summary>
public static class AppInfo
{
    public const string Title = "PinQuick";

    public const string Author = "KTYSoft";

    public const string Developer = "Kutay ÖZTÜRK";

    public const string Website = "ktysoft.com.tr";

    public const string Email = "info@ktysoft.com.tr";

    public const string SponsorUrl = "https://github.com/sponsors/ktysoft";

    public const string License = "MIT License";

    public static string Version
    {
        get
        {
            var attribute = typeof(AppInfo).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            var version = attribute?.InformationalVersion ?? "0.0.0";
            return version.Split('+', 2)[0];
        }
    }
}