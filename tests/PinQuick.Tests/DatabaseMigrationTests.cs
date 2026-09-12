using Microsoft.Data.Sqlite;
using PinQuick.Storage.Database;
using PinQuick.Storage.Repositories;

namespace PinQuick.Tests;

public sealed class DatabaseMigrationTests : IDisposable
{
    private readonly string _databasePath;
    private readonly string _connectionString;

    public DatabaseMigrationTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"pinquick-v1-{Guid.NewGuid():N}.db");
        _connectionString = $"Data Source={_databasePath}";
    }

    private void CreateV1Schema()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE SchemaVersion (Id INTEGER PRIMARY KEY, Version INTEGER NOT NULL, AppliedAt TEXT NOT NULL);
            INSERT INTO SchemaVersion (Id, Version, AppliedAt) VALUES (1, 1, '2026-01-01T00:00:00.0000000Z');

            CREATE TABLE Collections (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Icon TEXT NOT NULL DEFAULT '',
                Color TEXT NOT NULL DEFAULT '',
                SortOrder INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE TABLE Pins (
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

            INSERT INTO Collections (Name, Color, SortOrder, CreatedAt, UpdatedAt)
            VALUES ('İş', '#FF0000', 0, '2026-01-01T00:00:00.0000000Z', '2026-01-01T00:00:00.0000000Z');

            INSERT INTO Pins (Title, Type, Target, CollectionId, CreatedAt, UpdatedAt)
            VALUES ('Not', 0, 'C:\note.txt', 1, '2026-01-01T00:00:00.0000000Z', '2026-01-01T00:00:00.0000000Z');
            """;
        command.ExecuteNonQuery();
    }

    [Fact]
    public async Task Initialize_MigratesLegacyCollectionIdToPinCollections()
    {
        CreateV1Schema();
        var initializer = new DatabaseInitializer(_connectionString);

        initializer.Initialize();

        var pinRepository = new SqlitePinRepository(_connectionString);
        var pins = await pinRepository.GetAllAsync();
        var pin = Assert.Single(pins);
        Assert.Equal(1L, Assert.Single(pin.CollectionIds));
    }

    [Fact]
    public async Task Initialize_DoesNotDuplicateMembershipOnReinitialize()
    {
        CreateV1Schema();
        var initializer = new DatabaseInitializer(_connectionString);
        initializer.Initialize();
        initializer.Initialize();

        var pinRepository = new SqlitePinRepository(_connectionString);
        var pins = await pinRepository.GetAllAsync();
        var pin = Assert.Single(pins);
        Assert.Single(pin.CollectionIds);
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(_databasePath))
            {
                File.Delete(_databasePath);
            }
        }
        catch (IOException)
        {
        }
    }
}