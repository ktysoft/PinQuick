using PinQuick.Core.Services;
using PinQuick.Storage;
using PinQuick.Storage.Database;
using PinQuick.Storage.Repositories;
using PinQuick.Windows;

namespace PinQuick.App;

/// <summary>
/// Uygulama genelinde kullanılan servislerin tek noktası.
/// </summary>
public sealed class AppServices
{
    public PinManager PinManager { get; }

    public CollectionManager CollectionManager { get; }

    public ProcessLauncher ProcessLauncher { get; }

    public AppServices()
    {
        var connectionString = StoragePaths.CreateConnectionString();
        var initializer = new DatabaseInitializer(connectionString);
        initializer.Initialize();

        PinManager = new PinManager(new SqlitePinRepository(connectionString));
        CollectionManager = new CollectionManager(new SqliteCollectionRepository(connectionString));
        ProcessLauncher = new ProcessLauncher();
    }
}