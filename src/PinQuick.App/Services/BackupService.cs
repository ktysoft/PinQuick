using System.IO.Compression;
using PinQuick.Storage;

namespace PinQuick.App.Services;

/// <summary>
/// Uygulama verilerinin (SQLite veritabanı + ayarlar) ZIP yedeğini oluşturur
/// ve geri yükler.
/// </summary>
public static class BackupService
{
    public static string BackupDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PinQuick", "Backup");

    /// <summary>
    /// Yedek klasörüne zaman damgalı bir ZIP oluşturur ve dosya yolunu döndürür.
    /// </summary>
    public static string CreateBackup()
    {
        var backupDirectory = BackupDirectory;
        Directory.CreateDirectory(backupDirectory);

        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var archivePath = Path.Combine(backupDirectory, $"pinquick-backup-{stamp}.zip");

        using var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create);
        var databasePath = Path.Combine(StoragePaths.ResolveDataDirectory(), "pins.db");
        if (File.Exists(databasePath))
        {
            archive.CreateEntryFromFile(databasePath, "pins.db");
        }

        var settingsPath = AppSettings.SettingsPath;
        if (File.Exists(settingsPath))
        {
            archive.CreateEntryFromFile(settingsPath, "settings.json");
        }

        return archivePath;
    }

    /// <summary>
    /// ZIP yedeğindeki dosyaları gerçek veri konumlarına geri yazar.
    /// Geri yükleme sonrasında uygulamanın yeniden başlatılması gerekir.
    /// </summary>
    public static void RestoreBackup(string archivePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);

        using var archive = ZipFile.OpenRead(archivePath);
        var dataDirectory = StoragePaths.ResolveDataDirectory();
        Directory.CreateDirectory(dataDirectory);

        foreach (var entry in archive.Entries)
        {
            var destination = entry.FullName switch
            {
                "pins.db" => Path.Combine(dataDirectory, "pins.db"),
                "settings.json" => AppSettings.SettingsPath,
                _ => null,
            };

            if (destination is null)
            {
                continue;
            }

            var directory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            entry.ExtractToFile(destination, overwrite: true);
        }
    }
}