using System.Text.Json;

namespace PinQuick.App.Services;

/// <summary>
/// GitHub Releases üzerinden en son sürüm kontrolünün sonucu.
/// </summary>
public sealed class UpdateCheckResult
{
    public bool Succeeded { get; init; }

    public bool IsUpdateAvailable { get; init; }

    public string? CurrentVersion { get; init; }

    public string? LatestVersion { get; init; }

    public string? ReleaseUrl { get; init; }

    public string? ReleaseNotes { get; init; }
}

/// <summary>
/// GitHub Releases üzerinden en son sürümü kontrol eder.
/// </summary>
public static class UpdateService
{
    private const string RepoOwner = "ktysoft";

    private const string RepoName = "PinQuick";

    private const string LatestReleaseApi = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";

    private static readonly HttpClient HttpClient = CreateClient();

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"PinQuick/{AppInfo.Version}");
        return client;
    }

    public static async Task<UpdateCheckResult> CheckForUpdatesAsync()
    {
        try
        {
            var json = await HttpClient.GetStringAsync(LatestReleaseApi);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var latestVersion = root.TryGetProperty("tag_name", out var tag)
                ? tag.GetString()?.TrimStart('v', 'V')
                : null;
            var releaseUrl = root.TryGetProperty("html_url", out var url)
                ? url.GetString()
                : null;
            var releaseNotes = root.TryGetProperty("body", out var body)
                ? body.GetString()
                : null;

            var current = ParseVersion(AppInfo.Version);
            var latest = ParseVersion(latestVersion);

            return new UpdateCheckResult
            {
                Succeeded = true,
                CurrentVersion = AppInfo.Version,
                LatestVersion = latestVersion,
                ReleaseUrl = releaseUrl,
                ReleaseNotes = releaseNotes,
                IsUpdateAvailable = current is not null && latest is not null && latest > current,
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return new UpdateCheckResult
            {
                Succeeded = false,
                CurrentVersion = AppInfo.Version,
            };
        }
    }

    private static Version? ParseVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Version.TryParse(value, out var version))
        {
            return version;
        }

        // 1.3.0-beta2 gibi 3 rakamdan fazla bölüm içeren sürümleri normalleştir.
        var parts = value.Split(['.', '-', '+'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2
            && int.TryParse(parts[0], out var major)
            && int.TryParse(parts[1], out var minor))
        {
            var patch = parts.Length >= 3 && int.TryParse(parts[2], out var p) ? p : 0;
            return new Version(major, minor, patch);
        }

        return null;
    }
}