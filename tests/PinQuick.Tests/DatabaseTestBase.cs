using PinQuick.Core.Models;
using PinQuick.Core.Services;
using PinQuick.Storage.Database;
using PinQuick.Storage.Repositories;

namespace PinQuick.Tests;

public sealed class TestDatabase : IDisposable
{
    public string ConnectionString { get; }

    public string DatabasePath { get; }

    public TestDatabase()
    {
        DatabasePath = Path.Combine(Path.GetTempPath(), $"pinquick-tests-{Guid.NewGuid():N}.db");
        ConnectionString = $"Data Source={DatabasePath}";
        var initializer = new DatabaseInitializer(ConnectionString);
        initializer.Initialize();
    }

    public SqlitePinRepository CreatePinRepository() => new(ConnectionString);

    public SqliteCollectionRepository CreateCollectionRepository() => new(ConnectionString);

    public void Dispose()
    {
        try
        {
            if (File.Exists(DatabasePath))
            {
                File.Delete(DatabasePath);
            }
        }
        catch (IOException)
        {
        }
    }
}

public abstract class DatabaseTestBase : IDisposable
{
    protected TestDatabase Database { get; }

    protected PinManager PinManager { get; }

    protected CollectionManager CollectionManager { get; }

    protected DatabaseTestBase()
    {
        Database = new TestDatabase();
        PinManager = new PinManager(Database.CreatePinRepository());
        CollectionManager = new CollectionManager(Database.CreateCollectionRepository());
    }

    public void Dispose()
    {
        Database.Dispose();
    }

    protected static Pin CreatePin(string title = "Visual Studio Code", PinType type = PinType.Application, string target = @"C:\Program Files\Microsoft VS Code\Code.exe")
    {
        return new Pin
        {
            Title = title,
            Type = type,
            Target = target,
        };
    }
}