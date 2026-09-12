using Microsoft.Data.Sqlite;

namespace PinQuick.Storage.Database;

/// <summary>
/// SQLite veritabanını açar, şemayı oluşturur ve migration sistemini yönetir.
/// </summary>
public sealed class DatabaseInitializer
{
    private const int CurrentSchemaVersion = 2;
    private readonly string _connectionString;

    public DatabaseInitializer(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    public SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    /// <summary>
    /// Veritabanını hazırlar: tabloları oluşturur ve migration'ları uygular.
    /// </summary>
    public void Initialize()
    {
        using var connection = OpenConnection();

        CreateSchemaVersionTable(connection);
        CreateTables(connection);
        UpdateSchemaVersion(connection);
    }

    private static void CreateSchemaVersionTable(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS SchemaVersion (
                Id INTEGER PRIMARY KEY,
                Version INTEGER NOT NULL,
                AppliedAt TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    private static void CreateTables(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Collections (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Icon TEXT NOT NULL DEFAULT '',
                Color TEXT NOT NULL DEFAULT '',
                SortOrder INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Pins (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Description TEXT NOT NULL DEFAULT '',
                Type INTEGER NOT NULL,
                Target TEXT NOT NULL DEFAULT '',
                Arguments TEXT NOT NULL DEFAULT '',
                WorkingDirectory TEXT NOT NULL DEFAULT '',
                Icon TEXT NOT NULL DEFAULT '',
                CollectionId INTEGER NULL,
                IsFavorite INTEGER NOT NULL DEFAULT 0,
                IsEnabled INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                LastUsedAt TEXT NULL,
                UseCount INTEGER NOT NULL DEFAULT 0,
                SortOrder INTEGER NOT NULL DEFAULT 0,
                Tags TEXT NOT NULL DEFAULT '',
                RunAsAdministrator INTEGER NOT NULL DEFAULT 0,
                OpenWith TEXT NOT NULL DEFAULT '',
                CustomColor TEXT NOT NULL DEFAULT '',
                CustomShortcut TEXT NOT NULL DEFAULT '',
                FOREIGN KEY (CollectionId) REFERENCES Collections(Id) ON DELETE SET NULL
            );

            CREATE INDEX IF NOT EXISTS IX_Pins_Title ON Pins(Title);
            CREATE INDEX IF NOT EXISTS IX_Pins_Type ON Pins(Type);
            CREATE INDEX IF NOT EXISTS IX_Pins_CollectionId ON Pins(CollectionId);
            CREATE INDEX IF NOT EXISTS IX_Pins_IsFavorite ON Pins(IsFavorite);
            CREATE INDEX IF NOT EXISTS IX_Pins_Target ON Pins(Target);

            CREATE TABLE IF NOT EXISTS PinCollections (
                PinId INTEGER NOT NULL,
                CollectionId INTEGER NOT NULL,
                PRIMARY KEY (PinId, CollectionId),
                FOREIGN KEY (PinId) REFERENCES Pins(Id) ON DELETE CASCADE,
                FOREIGN KEY (CollectionId) REFERENCES Collections(Id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS IX_PinCollections_CollectionId ON PinCollections(CollectionId);
            """;
        command.ExecuteNonQuery();
    }

    private static void UpdateSchemaVersion(SqliteConnection connection)
    {
        var current = GetSchemaVersion(connection);
        if (current < 2)
        {
            MigrateToPinCollections(connection);
        }

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO SchemaVersion (Id, Version, AppliedAt) VALUES (1, $version, $appliedAt)
            ON CONFLICT(Id) DO UPDATE SET Version = excluded.Version, AppliedAt = excluded.AppliedAt;
            """;
        command.Parameters.AddWithValue("$version", CurrentSchemaVersion);
        command.Parameters.AddWithValue("$appliedAt", DateTime.UtcNow.ToString("o"));
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Eski tek koleksiyon modelindeki (Pins.CollectionId) ilişkileri
    /// PinCollections birleştirme tablosuna taşır.
    /// </summary>
    private static void MigrateToPinCollections(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO PinCollections (PinId, CollectionId)
            SELECT Id, CollectionId FROM Pins
            WHERE CollectionId IS NOT NULL;
            """;
        command.ExecuteNonQuery();
    }

    private static int GetSchemaVersion(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COALESCE((SELECT Version FROM SchemaVersion WHERE Id = 1), 0)";
        return Convert.ToInt32(command.ExecuteScalar());
    }
}