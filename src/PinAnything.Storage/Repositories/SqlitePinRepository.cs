using Microsoft.Data.Sqlite;
using PinAnything.Core.Abstractions;
using PinAnything.Core.Models;

namespace PinAnything.Storage.Repositories;

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
        return await reader.ReadAsync(cancellationToken) ? ReadPin(reader) : null;
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

        return pins;
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

        return Convert.ToInt64(result);
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

    public async Task<IReadOnlyList<Pin>> SearchAsync(string query, CancellationToken cancellationToken = default)
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
            WHERE Title LIKE $term COLLATE NOCASE
               OR Description LIKE $term COLLATE NOCASE
               OR Tags LIKE $term COLLATE NOCASE
               OR Target LIKE $term COLLATE NOCASE
               OR Type = $type
            ORDER BY IsFavorite DESC,
                     CASE WHEN Title LIKE $term COLLATE NOCASE THEN 0 ELSE 1 END,
                     Title COLLATE NOCASE ASC
            """;
        command.Parameters.AddWithValue("$term", $"%{query}%");
        command.Parameters.AddWithValue("$type", TryParsePinType(query));

        var pins = new List<Pin>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            pins.Add(ReadPin(reader));
        }

        return pins;
    }

    public async Task ClearCollectionAsync(long collectionId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Pins SET CollectionId = NULL WHERE CollectionId = $collectionId";
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

    private static int TryParsePinType(string value)
    {
        return Enum.TryParse<PinType>(value, ignoreCase: true, out var result) ? (int)result : -1;
    }
}