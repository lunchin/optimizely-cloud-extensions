using lunchin.Optimizely.Cloud.Extensions.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace lunchin.Optimizely.Cloud.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void UselunchinOptimizelyCloudExtensions(this IApplicationBuilder applicationBuilder)
    {
        if (!applicationBuilder.ApplicationServices.TryGetExistingInstance<IOptions<ExtensionsOptions>>(out var options))
        {
            return;
        }

        if (options.Value.SettingsEnabled)
        {
            applicationBuilder.ApplicationServices.GetInstance<ISettingsService>()?.InitializeSettings();
        }

        if (options.Value.MasterLanguageEnabled)
        {
            InitializeDatabase().GetAwaiter().GetResult();
        }
    }

    private static async Task InitializeDatabase()
    {
        var databaseVersions = DatabaseVersion.Get();

        if (databaseVersions == null)
        {
            var result = await DatabaseUtilities.RunUpgradeScript("EPiServerDB",
                "lunchin.Optimizely.Cloud.Extensions.Database.MasterLanguage",
                Constants.MasterLanguageDatabaseVersionNumber);

            if (!result)
            {
                return;
            }
            databaseVersions = new DatabaseVersion()
            {
                MasterLanguageDatabaseVersion = Constants.MasterLanguageDatabaseVersionNumber
            };
            databaseVersions.Save();
        }
        else if (!(databaseVersions.MasterLanguageDatabaseVersion ?? "").Equals(Constants.MasterLanguageDatabaseVersionNumber))
        {
            var result = await DatabaseUtilities.RunUpgradeScript("EPiServerDB",
                "lunchin.Optimizely.Cloud.Extensions.Database.MasterLanguage",
                Constants.MasterLanguageDatabaseVersionNumber,
                databaseVersions.MasterLanguageDatabaseVersion ?? "0.0.0");

            if (!result)
            {
                return;
            }
            databaseVersions.MasterLanguageDatabaseVersion = Constants.MasterLanguageDatabaseVersionNumber;
            databaseVersions.Save();
        }
    }
}
