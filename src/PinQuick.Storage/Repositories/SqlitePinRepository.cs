using Microsoft.Data.Sqlite;
using PinQuick.Core.Abstractions;
using PinQuick.Core.Models;

namespace PinQuick.Storage.Repositories;

public sealed class SqlitePinRepository : IPinRepository
{
    private readonly string _connectionString;

    public SqlitePinRepository(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    public async Task<Pin?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, Title, Description, Type, Target, Arguments, WorkingDirectory, Icon,
                   CollectionId, IsFavorite, IsEnabled, CreatedAt, UpdatedAt, LastUsedAt,
                   UseCount, SortOrder, Tags, RunAsAdministrator, OpenWith, CustomColor, CustomShortcut
            FROM Pins
            WHERE Id = $id
            """;
        command.Parameters.AddWithValue("$id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        Pin? pin = await reader.ReadAsync(cancellationToken) ? ReadPin(reader) : null;

        if (pin is not null)
        {
            await LoadMembershipsAsync(connection, pin, cancellationToken);
        }

        return pin;
    }

    public async Task<IReadOnlyList<Pin>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, Title, Description, Type, Target, Arguments, WorkingDirectory, Icon,
                   CollectionId, IsFavorite, IsEnabled, CreatedAt, UpdatedAt, LastUsedAt,
                   UseCount, SortOrder, Tags, RunAsAdministrator, OpenWith, CustomColor, CustomShortcut
            FROM Pins
            ORDER BY SortOrder ASC, Title COLLATE NOCASE ASC
            """;

        var pins = new List<Pin>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            pins.Add(ReadPin(reader));
        }

        await LoadMembershipsAsync(connection, pins, cancellationToken);
        return pins;
    }

    /// <summary>
    /// Pinlerin koleksiyon üyeliklerini (PinCollections) yükleyip her pinin
    /// CollectionIds listesine yazar.
    /// </summary>
    private static async Task LoadMembershipsAsync(SqliteConnection connection, IEnumerable<Pin> pins, CancellationToken cancellationToken)
    {
        var pinById = pins.ToDictionary(p => p.Id);
        if (pinById.Count == 0)
        {
            return;
        }

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT PinId, CollectionId FROM PinCollections";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var pinId = reader.GetInt64(0);
            if (pinById.TryGetValue(pinId, out var pin))
            {
                pin.CollectionIds.Add(reader.GetInt64(1));
            }
        }
    }

    /// <summary>
    /// Tek pinin koleksiyon üyeliklerini yükler.
    /// </summary>
    private static async Task LoadMembershipsAsync(SqliteConnection connection, Pin pin, CancellationToken cancellationToken)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT CollectionId FROM PinCollections WHERE PinId = $pinId";
        command.Parameters.AddWithValue("$pinId", pin.Id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            pin.CollectionIds.Add(reader.GetInt64(0));
        }
    }

    public async Task<long> AddAsync(Pin pin, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO Pins (Title, Description, Type, Target, Arguments, WorkingDirectory, Icon,
                              CollectionId, IsFavorite, IsEnabled, CreatedAt, UpdatedAt, LastUsedAt,
                              UseCount, SortOrder, Tags, RunAsAdministrator, OpenWith, CustomColor, CustomShortcut)
            VALUES ($title, $description, $type, $target, $arguments, $workingDirectory, $icon,
                    $collectionId, $isFavorite, $isEnabled, $createdAt, $updatedAt, $lastUsedAt,
                    $useCount, $sortOrder, $tags, $runAsAdministrator, $openWith, $customColor, $customShortcut);
            SELECT last_insert_rowid();
            """;
        AddPinParameters(command, pin);
        command.Parameters.AddWithValue("$lastUsedAt", (object?)pin.LastUsedAt?.ToString("o") ?? DBNull.Value);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is null || result is DBNull)
        {
            throw new InvalidOperationException("Pin eklenirken kimlik alınamadı.");
        }

        var id = Convert.ToInt64(result);

        foreach (var collectionId in pin.CollectionIds.Distinct())
        {
            await InsertMembershipAsync(connection, id, collectionId, cancellationToken);
        }

        return id;
    }

    private static async Task InsertMembershipAsync(SqliteConnection connection, long pinId, long collectionId, CancellationToken cancellationToken)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT OR IGNORE INTO PinCollections (PinId, CollectionId)
            VALUES ($pinId, $collectionId);
            """;
        command.Parameters.AddWithValue("$pinId", pinId);
        command.Parameters.AddWithValue("$collectionId", collectionId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(Pin pin, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE Pins
            SET Title = $title, Description = $description, Type = $type, Target = $target,
                Arguments = $arguments, WorkingDirectory = $workingDirectory, Icon = $icon,
                CollectionId = $collectionId, IsFavorite = $isFavorite, IsEnabled = $isEnabled,
                CreatedAt = $createdAt, UpdatedAt = $updatedAt, LastUsedAt = $lastUsedAt,
                UseCount = $useCount, SortOrder = $sortOrder, Tags = $tags,
                RunAsAdministrator = $runAsAdministrator, OpenWith = $openWith,
                CustomColor = $customColor, CustomShortcut = $customShortcut
            WHERE Id = $id
            """;
        AddPinParameters(command, pin);
        command.Parameters.AddWithValue("$id", pin.Id);
        command.Parameters.AddWithValue("$lastUsedAt", (object?)pin.LastUsedAt?.ToString("o") ?? DBNull.Value);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected == 0)
        {
            throw new KeyNotFoundException($"Id değeri {pin.Id} olan pin bulunamadı.");
        }
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Pins WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected == 0)
        {
            throw new KeyNotFoundException($"Id değeri {id} olan pin bulunamadı.");
        }
    }

    public async Task ReorderAsync(IReadOnlyList<(long Id, int SortOrder)> orderedItems, CancellationToken cancellationToken = default)
    {
        if (orderedItems is null || orderedItems.Count == 0)
        {
            return;
        }

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = connection.BeginTransaction();

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            UPDATE Pins
            SET SortOrder = $sortOrder, UpdatedAt = $updatedAt
            WHERE Id = $id
            """;

        var sortOrderParameter = command.Parameters.Add("$sortOrder", SqliteType.Integer);
        var updatedAtParameter = command.Parameters.Add("$updatedAt", SqliteType.Text);
        var idParameter = command.Parameters.Add("$id", SqliteType.Integer);

        var updatedAt = DateTime.UtcNow.ToString("o");
        foreach (var (id, sortOrder) in orderedItems)
        {
            sortOrderParameter.Value = sortOrder;
            updatedAtParameter.Value = updatedAt;
            idParameter.Value = id;
            var affected = await command.ExecuteNonQueryAsync(cancellationToken);
            if (affected == 0)
            {
                throw new KeyNotFoundException($"Id değeri {id} olan pin bulunamadı.");
            }
        }

        transaction.Commit();
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Pins";

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result ?? 0);
    }

    public async Task<Pin?> FindDuplicateAsync(PinType type, string target, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, Title, Description, Type, Target, Arguments, WorkingDirectory, Icon,
                   CollectionId, IsFavorite, IsEnabled, CreatedAt, UpdatedAt, LastUsedAt,
                   UseCount, SortOrder, Tags, RunAsAdministrator, OpenWith, CustomColor, CustomShortcut
            FROM Pins
            WHERE Type = $type AND Target = $target COLLATE NOCASE
            ORDER BY Id
            LIMIT 1
            """;
        command.Parameters.AddWithValue("$type", (int)type);
        command.Parameters.AddWithValue("$target", target);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadPin(reader) : null;
    }

    public async Task AddToCollectionAsync(long pinId, long collectionId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await InsertMembershipAsync(connection, pinId, collectionId, cancellationToken);
    }

    public async Task RemoveFromCollectionAsync(long pinId, long collectionId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            DELETE FROM PinCollections
            WHERE PinId = $pinId AND CollectionId = $collectionId
            """;
        command.Parameters.AddWithValue("$pinId", pinId);
        command.Parameters.AddWithValue("$collectionId", collectionId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ClearCollectionAsync(long collectionId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM PinCollections WHERE CollectionId = $collectionId";
        command.Parameters.AddWithValue("$collectionId", collectionId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddPinParameters(SqliteCommand command, Pin pin)
    {
        command.Parameters.AddWithValue("$title", pin.Title);
        command.Parameters.AddWithValue("$description", pin.Description);
        command.Parameters.AddWithValue("$type", (int)pin.Type);
        command.Parameters.AddWithValue("$target", pin.Target);
        command.Parameters.AddWithValue("$arguments", pin.Arguments);
        command.Parameters.AddWithValue("$workingDirectory", pin.WorkingDirectory);
        command.Parameters.AddWithValue("$icon", pin.Icon);
        command.Parameters.AddWithValue("$collectionId", (object?)pin.CollectionId ?? DBNull.Value);
        command.Parameters.AddWithValue("$isFavorite", pin.IsFavorite ? 1 : 0);
        command.Parameters.AddWithValue("$isEnabled", pin.IsEnabled ? 1 : 0);
        command.Parameters.AddWithValue("$createdAt", pin.CreatedAt.ToString("o"));
        command.Parameters.AddWithValue("$updatedAt", pin.UpdatedAt.ToString("o"));
        command.Parameters.AddWithValue("$useCount", pin.UseCount);
        command.Parameters.AddWithValue("$sortOrder", pin.SortOrder);
        command.Parameters.AddWithValue("$tags", pin.Tags);
        command.Parameters.AddWithValue("$runAsAdministrator", pin.RunAsAdministrator ? 1 : 0);
        command.Parameters.AddWithValue("$openWith", pin.OpenWith);
        command.Parameters.AddWithValue("$customColor", pin.CustomColor);
        command.Parameters.AddWithValue("$customShortcut", pin.CustomShortcut);
    }

    private static Pin ReadPin(SqliteDataReader reader)
    {
        var typeValue = reader.GetInt32(3);
        return new Pin
        {
            Id = reader.GetInt64(0),
            Title = reader.GetString(1),
            Description = reader.GetString(2),
            Type = Enum.IsDefined(typeof(PinType), typeValue) ? (PinType)typeValue : PinType.Custom,
            Target = reader.GetString(4),
            Arguments = reader.GetString(5),
            WorkingDirectory = reader.GetString(6),
            Icon = reader.GetString(7),
            CollectionId = reader.IsDBNull(8) ? null : reader.GetInt64(8),
            IsFavorite = reader.GetInt32(9) == 1,
            IsEnabled = reader.GetInt32(10) == 1,
            CreatedAt = ParseDateTime(reader.GetString(11)),
            UpdatedAt = ParseDateTime(reader.GetString(12)),
            LastUsedAt = reader.IsDBNull(13) ? null : ParseDateTime(reader.GetString(13)),
            UseCount = reader.GetInt32(14),
            SortOrder = reader.GetInt32(15),
            Tags = reader.GetString(16),
            RunAsAdministrator = reader.GetInt32(17) == 1,
            OpenWith = reader.GetString(18),
            CustomColor = reader.GetString(19),
            CustomShortcut = reader.GetString(20),
        };
    }

    private static DateTime ParseDateTime(string value)
        => DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed)
            ? parsed
            : DateTime.MinValue;
}