using Microsoft.Data.Sqlite;
using PinQuick.Core.Abstractions;
using PinQuick.Core.Models;

namespace PinQuick.Storage.Repositories;

public sealed class SqliteCollectionRepository : ICollectionRepository
{
    private readonly string _connectionString;

    public SqliteCollectionRepository(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    public async Task<Collection?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, Name, Icon, Color, SortOrder, CreatedAt, UpdatedAt
            FROM Collections
            WHERE Id = $id
            """;
        command.Parameters.AddWithValue("$id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadCollection(reader) : null;
    }

    public async Task<IReadOnlyList<Collection>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, Name, Icon, Color, SortOrder, CreatedAt, UpdatedAt
            FROM Collections
            ORDER BY SortOrder ASC, Name COLLATE NOCASE ASC
            """;

        var collections = new List<Collection>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            collections.Add(ReadCollection(reader));
        }

        return collections;
    }

    public async Task<long> AddAsync(Collection collection, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO Collections (Name, Icon, Color, SortOrder, CreatedAt, UpdatedAt)
            VALUES ($name, $icon, $color, $sortOrder, $createdAt, $updatedAt);
            SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("$name", collection.Name);
        command.Parameters.AddWithValue("$icon", collection.Icon);
        command.Parameters.AddWithValue("$color", collection.Color);
        command.Parameters.AddWithValue("$sortOrder", collection.SortOrder);
        command.Parameters.AddWithValue("$createdAt", collection.CreatedAt.ToString("o"));
        command.Parameters.AddWithValue("$updatedAt", collection.UpdatedAt.ToString("o"));

        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is null || result is DBNull)
        {
            throw new InvalidOperationException("Koleksiyon eklenirken kimlik alınamadı.");
        }

        return Convert.ToInt64(result);
    }

    public async Task UpdateAsync(Collection collection, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE Collections
            SET Name = $name, Icon = $icon, Color = $color,
                SortOrder = $sortOrder, UpdatedAt = $updatedAt
            WHERE Id = $id
            """;
        command.Parameters.AddWithValue("$name", collection.Name);
        command.Parameters.AddWithValue("$icon", collection.Icon);
        command.Parameters.AddWithValue("$color", collection.Color);
        command.Parameters.AddWithValue("$sortOrder", collection.SortOrder);
        command.Parameters.AddWithValue("$updatedAt", collection.UpdatedAt.ToString("o"));
        command.Parameters.AddWithValue("$id", collection.Id);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected == 0)
        {
            throw new KeyNotFoundException($"Id değeri {collection.Id} olan koleksiyon bulunamadı.");
        }
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Collections WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected == 0)
        {
            throw new KeyNotFoundException($"Id değeri {id} olan koleksiyon bulunamadı.");
        }
    }

    private static Collection ReadCollection(SqliteDataReader reader)
    {
        return new Collection
        {
            Id = reader.GetInt64(0),
            Name = reader.GetString(1),
            Icon = reader.GetString(2),
            Color = reader.GetString(3),
            SortOrder = reader.GetInt32(4),
            CreatedAt = ParseDateTime(reader.GetString(5)),
            UpdatedAt = ParseDateTime(reader.GetString(6)),
        };
    }

    private static DateTime ParseDateTime(string value)
        => DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed)
            ? parsed
            : DateTime.MinValue;
}