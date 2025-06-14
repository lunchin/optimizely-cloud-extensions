using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace lunchin.Optimizely.Cloud.Extensions;

[EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true, StoreName = "loceDatabaseVersion")]
public class DatabaseVersion
{
    public Identity? Id { get; set; }

    public string? CouponDatabaseVersion { get; set; }

    public string? MasterLanguageDatabaseVersion { get; set; }

    public void Save()
    {
        var store = typeof(DatabaseVersion).GetStore();
        var unused = store.Save(this);
    }

    public static DatabaseVersion? Get()
    {
        var store = typeof(DatabaseVersion).GetStore();
        return store?.Items<DatabaseVersion>()
            .FirstOrDefault();
    }
}
