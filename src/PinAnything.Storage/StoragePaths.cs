namespace PinAnything.Storage;

/// <summary>
/// Uygulama veri dizini ve SQLite bağlantı dizisi oluşturmaya yardımcı olur.
/// Portable modda veriler uygulamanın bulunduğu dizindeki Data klasöründe tutulur.
/// </summary>
public static class StoragePaths
{
    /// <summary>
    /// Kullanıcıya özel veya portable veri dizinini döndürür.
    /// </summary>
    /// <param name="baseDirectory">Portable modda verilerin tutulacağı temel dizin.</param>
    public static string ResolveDataDirectory(string? baseDirectory = null)
    {
        baseDirectory ??= Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PinAnything");

        return Path.Combine(baseDirectory, "Data");
    }

    /// <summary>
    /// SQLite veritabanı dosyasının tam yolunu döndürür.
    /// </summary>
    public static string ResolveDatabasePath(string? baseDirectory = null)
    {
        var dataDirectory = ResolveDataDirectory(baseDirectory);
        Directory.CreateDirectory(dataDirectory);
        return Path.Combine(dataDirectory, "pins.db");
    }

    /// <summary>
    /// SQLite bağlantı dizisi oluşturur.
    /// </summary>
    public static string CreateConnectionString(string? baseDirectory = null)
    {
        var databasePath = ResolveDatabasePath(baseDirectory);
        var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWriteCreate,
            Cache = Microsoft.Data.Sqlite.SqliteCacheMode.Shared,
        };
        return builder.ConnectionString;
    }
}