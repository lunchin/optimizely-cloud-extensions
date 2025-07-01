using EPiServer.Shell.ObjectEditing;
using lunchin.Optimizely.Cloud.Extensions.Settings;
using lunchin.Optimizely.Cloud.Extensions.Taxonomy;
using lunchin.Optimizely.Cloud.Extensions.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.Win32;

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

        if (options.Value.TaxonomyEnabled)
        {
            var contentService = applicationBuilder.ApplicationServices.GetInstance<ContentRootService>();
            var registeredRoots = applicationBuilder.ApplicationServices.GetInstance<IContentRepository>().GetItems(contentService.List(), []);
            var settingsRootRegistered = registeredRoots.Any(x => x.ContentGuid == ClassificationFolder.ClassificationRootGuid && x.Name.Equals(ClassificationFolder.ClassificationRootName));

            if (!settingsRootRegistered)
            {
                contentService.Register<ClassificationFolder>(ClassificationFolder.ClassificationRootName, ClassificationFolder.ClassificationRootGuid, ContentReference.RootPage);
            }
        }

        if (options.Value.HideDefaultCategoryEnabled)
        {
            var registry = applicationBuilder.ApplicationServices.GetInstance<MetadataHandlerRegistry>();
            registry.RegisterMetadataHandler(typeof(ContentData), new HideCategoryPropertyExtender(applicationBuilder.ApplicationServices.GetInstance<IOptions<ExtensionsOptions>>()));
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
