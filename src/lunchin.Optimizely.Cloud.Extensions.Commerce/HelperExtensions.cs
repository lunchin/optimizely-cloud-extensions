using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Reporting.Order.ReportingModels;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Commerce.Storage;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Mediachase.BusinessFoundation.Data.Business;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Pricing;

namespace lunchin.Optimizely.Cloud.Extensions.Commerce;

public static class HelperExtensions
{
    private static readonly Lazy<AssetUrlResolver> _assetResolver = new(() => ServiceLocator.Current.GetInstance<AssetUrlResolver>());
    private static readonly Lazy<IOrderGroupFactory> _orderGroupFactory = new(() => ServiceLocator.Current.GetInstance<IOrderGroupFactory>());
    private static readonly Lazy<IContentLoader> _contentLoader = new(() => ServiceLocator.Current.GetInstance<IContentLoader>());
    private static readonly Lazy<ReferenceConverter> _referenceConverter = new (() => ServiceLocator.Current.GetInstance<ReferenceConverter>());
    private static readonly Lazy<IPriceService> _priceService = new (() => ServiceLocator.Current.GetInstance<IPriceService>());
    private static readonly Lazy<IRelationRepository> _relationRepository = new (() => ServiceLocator.Current.GetInstance<IRelationRepository>());

    public static string GetString(this IExtendedProperties extendedProperties, string fieldName) => DefaultIfNull(extendedProperties.Properties[fieldName], string.Empty);

    public static bool GetBool(this IExtendedProperties extendedProperties, string fieldName) => DefaultIfNull(extendedProperties.Properties[fieldName], false);

    public static Guid GetGuid(this IExtendedProperties extendedProperties, string fieldName) => DefaultIfNull(extendedProperties.Properties[fieldName], Guid.Empty);

    public static int GetInt32(this IExtendedProperties extendedProperties, string fieldName) => DefaultIfNull(extendedProperties.Properties[fieldName], default(int));

    public static DateTime GetDateTime(this IExtendedProperties extendedProperties, string fieldName) => DefaultIfNull(extendedProperties.Properties[fieldName], DateTime.MaxValue);

    public static decimal GetDecimal(this IExtendedProperties extendedProperties, string fieldName) => DefaultIfNull(extendedProperties.Properties[fieldName], default(decimal));

    public static string? GetStringValue(this EntityObject item, string fieldName) => item.GetStringValue(fieldName, string.Empty);

    public static string? GetStringValue(this EntityObject item, string fieldName, string defaultValue) => item[fieldName] != null ? item[fieldName].ToString() : defaultValue;

    public static DateTime GetDateTimeValue(this EntityObject item, string fieldName) => item.GetDateTimeValue(fieldName, DateTime.MinValue);

    public static DateTime GetDateTimeValue(this EntityObject item, string fieldName, DateTime defaultValue) => item[fieldName] == null ? defaultValue : DateTime.TryParse(item[fieldName].ToString(), out var retVal) ? retVal : defaultValue;

    public static int GetIntegerValue(this EntityObject item, string fieldName) => item.GetIntegerValue(fieldName, 0);

    public static int GetIntegerValue(this EntityObject item, string fieldName, int defaultValue) => item[fieldName] == null ? defaultValue : int.TryParse(item[fieldName].ToString(), out var retVal) ? retVal : defaultValue;

    public static float GetFloatValue(this EntityObject item, string fieldName) => item.GetFloatValue(fieldName, 0);

    public static float GetFloatValue(this EntityObject item, string fieldName, float defaultValue) => item[fieldName] == null ? defaultValue : float.TryParse(item[fieldName].ToString(), out var retVal) ? retVal : defaultValue;

    public static decimal GetDecimalValue(this EntityObject item, string fieldName) => item.GetDecimalValue(fieldName, 0);

    public static decimal GetDecimalValue(this EntityObject item, string fieldName, decimal defaultValue) => item[fieldName] == null ? defaultValue : decimal.TryParse(item[fieldName].ToString(), out var retVal) ? retVal : defaultValue;

    public static bool GetBoolValue(this EntityObject item, string fieldName) => item.GetBoolValue(fieldName, false);

    public static bool GetBoolValue(this EntityObject item, string fieldName, bool defaultValue) => item[fieldName] == null ? defaultValue : bool.TryParse(item[fieldName].ToString(), out var retVal) ? retVal : defaultValue;

    public static Guid GetGuidValue(this EntityObject item, string fieldName) => item.GetGuidValue(fieldName, Guid.Empty);

    public static Guid GetGuidValue(this EntityObject item, string fieldName, Guid defaultValue) => item[fieldName] == null ? defaultValue : Guid.TryParse(item[fieldName].ToString(), out var retVal) ? retVal : defaultValue;

    private static T DefaultIfNull<T>(object? val, T defaultValue) => val == null || val == DBNull.Value ? defaultValue : (T)val;

    public static string GetDefaultAsset<T>(this IAssetContainer assetContainer)
            where T : IContentMedia
    {
        var url = _assetResolver.Value.GetAssetUrl<T>(assetContainer);
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.PathAndQuery : url;
    }

    public static IList<string> GetAssets<T>(this IAssetContainer assetContainer,
        IContentLoader contentLoader, UrlResolver urlResolver)
        where T : IContentMedia
    {
        var assets = new List<string>();
        if (assetContainer.CommerceMediaCollection != null)
        {
            assets.AddRange(assetContainer.CommerceMediaCollection
                .Where(x => ValidateCorrectType<T>(x.AssetLink, contentLoader))
                .Select(media => urlResolver.GetUrl(media.AssetLink, null, new VirtualPathArguments() { ContextMode = ContextMode.Default })));
        }

        if (!assets.Any())
        {
            assets.Add(string.Empty);
        }

        return assets;
    }

    public static IList<KeyValuePair<string, string>> GetAssetsWithType(this IAssetContainer assetContainer,
        IContentLoader contentLoader, UrlResolver urlResolver)
    {
        var assets = new List<KeyValuePair<string, string>>();
        if (assetContainer.CommerceMediaCollection != null)
        {
            assets.AddRange(
                assetContainer.CommerceMediaCollection
                .Select(media =>
                {
                    if (contentLoader.TryGet<IContentMedia>(media.AssetLink, out var contentMedia))
                    {
                        var type = "Image";
                        var url = urlResolver.GetUrl(media.AssetLink, null, new VirtualPathArguments() { ContextMode = ContextMode.Default });
                        if (contentMedia is IContentVideo)
                        {
                            type = "Video";
                        }

                        return new KeyValuePair<string, string>(type, url);
                    }

                    return new KeyValuePair<string, string>(string.Empty, string.Empty);
                })
                .Where(x => x.Key != string.Empty)
            );
        }

        return assets;
    }

    public static IList<MediaData?> GetAssetsMediaData(this IAssetContainer assetContainer, IContentLoader contentLoader, string groupName = "") => assetContainer.CommerceMediaCollection != null
            ? assetContainer.CommerceMediaCollection
                .Where(x => string.IsNullOrEmpty(groupName) || x.GroupName == groupName)
                .Select(x => contentLoader.Get<IContent>(x.AssetLink) as MediaData)
                .Where(x => x != null)
                .ToList()
            : (IList<MediaData?>)[];

    public static void AddValidationIssues(this Dictionary<ILineItem, List<ValidationIssue>> issues, ILineItem lineItem, ValidationIssue issue)
    {
        if (!issues.ContainsKey(lineItem))
        {
            issues.Add(lineItem, []);
        }

        if (!issues[lineItem].Contains(issue))
        {
            issues[lineItem].Add(issue);
        }
    }

    public static bool HasItemBeenRemoved(this Dictionary<ILineItem, List<ValidationIssue>> issuesPerLineItem, ILineItem lineItem) => issuesPerLineItem.TryGetValue(lineItem, out var issues)
        && issues.Any(x => x is ValidationIssue.RemovedDueToInactiveWarehouse or
                    ValidationIssue.RemovedDueToCodeMissing or
                    ValidationIssue.RemovedDueToInsufficientQuantityInInventory or
                    ValidationIssue.RemovedDueToInvalidPrice or
                    ValidationIssue.RemovedDueToMissingInventoryInformation or
                    ValidationIssue.RemovedDueToNotAvailableInMarket or
                    ValidationIssue.RemovedDueToUnavailableCatalog or
                    ValidationIssue.RemovedDueToUnavailableItem);

    public static IOrderAddress ConvertToOrderAddress(this CustomerAddress address, IOrderGroup order)
    {
        var newAddress = _orderGroupFactory.Value.CreateOrderAddress(order);
        newAddress.City = address.City;
        newAddress.CountryCode = address.CountryCode;
        newAddress.CountryName = address.CountryName;
        newAddress.DaytimePhoneNumber = address.DaytimePhoneNumber;
        newAddress.Email = address.Email;
        newAddress.EveningPhoneNumber = address.EveningPhoneNumber;
        newAddress.FirstName = address.FirstName;
        newAddress.LastName = address.LastName;
        newAddress.Line1 = address.Line1;
        newAddress.Line2 = address.Line2;
        newAddress.Id = address.Name;
        newAddress.PostalCode = address.PostalCode;
        newAddress.RegionName = address.RegionName;
        newAddress.RegionCode = address.RegionCode;
        return newAddress;
    }

    public static Price GetDefaultPrice(this ContentReference contentLink, MarketId marketId, Currency currency, DateTime validOn)
    {
        var catalogKey = new CatalogKey(_referenceConverter.Value.GetCode(contentLink));

        var priceValue = _priceService.Value.GetPrices(marketId, validOn, catalogKey, new PriceFilter() { Currencies = new[] { currency } })
            .OrderBy(x => x.UnitPrice).FirstOrDefault();
        return priceValue == null ? new Price() : new Price(priceValue);
    }

    public static IEnumerable<Price> GetPrices(this ContentReference entryContents,
        MarketId marketId, PriceFilter priceFilter) => new[] { entryContents }.GetPrices(marketId, priceFilter);

    public static IEnumerable<Price> GetPrices(this IEnumerable<ContentReference> entryContents, MarketId marketId, PriceFilter priceFilter)
    {
        var customerPricingList = priceFilter.CustomerPricing != null
            ? priceFilter.CustomerPricing.Where(x => x != null).ToList()
            : Enumerable.Empty<CustomerPricing>().ToList();

        var entryContentsList = entryContents.Where(x => x != null).ToList();

        var catalogKeys = entryContentsList.Select(GetCatalogKey);
        IEnumerable<IPriceValue> priceCollection;
        if (marketId == MarketId.Empty && (!customerPricingList.Any() ||
                                           customerPricingList.Any(x => string.IsNullOrEmpty(x.PriceCode))))
        {
            priceCollection = _priceService.Value.GetCatalogEntryPrices(catalogKeys);
        }
        else
        {
            var customerPricingsWithPriceCode =
                customerPricingList.Where(x => !string.IsNullOrEmpty(x.PriceCode)).ToList();
            if (customerPricingsWithPriceCode.Any())
            {
                priceFilter.CustomerPricing = customerPricingsWithPriceCode;
            }

            priceCollection = _priceService.Value.GetPrices(marketId, DateTime.UtcNow, catalogKeys, priceFilter);

            // if the entry has no price without sale code
            if (!priceCollection.Any())
            {
                priceCollection = _priceService.Value.GetCatalogEntryPrices(catalogKeys)
                   .Where(x => x.ValidFrom <= DateTime.Now && (!x.ValidUntil.HasValue || x.ValidUntil.Value >= DateTime.Now))
                   .Where(x => x.MarketId == marketId);
            }
        }

        return priceCollection.Select(x => new Price(x));
    }
    public static CatalogKey GetCatalogKey(this EntryContentBase productContent) => new CatalogKey(productContent.Code);

    public static CatalogKey GetCatalogKey(this ContentReference contentReference) => new CatalogKey(_referenceConverter.Value.GetCode(contentReference));

    public static T? GetEntryContent<T>(string code) where T : EntryContentBase
    {
        var entryContentLink = _referenceConverter.Value.GetContentLink(code);
        if (ContentReference.IsNullOrEmpty(entryContentLink))
        {
            return null;
        }

        return _contentLoader.Value.Get<T>(entryContentLink);
    }

    public static string GetCode(this ContentReference contentLink) => _referenceConverter.Value.GetCode(contentLink);

    public static EntryContentBase GetEntryContent(this CatalogKey catalogKey)
    {
        var entryContentLink = _referenceConverter.Value
            .GetContentLink(catalogKey.CatalogEntryCode, CatalogContentType.CatalogEntry);

        return _contentLoader.Value.Get<EntryContentBase>(entryContentLink);
    }

    public static IEnumerable<VariationContent> GetAllVariants(this ContentReference contentLink)
    {
        return GetAllVariants<VariationContent>(contentLink);
    }

    public static IEnumerable<VariationContent> VariationContents(this ProductContent productContent)
    {
        return _contentLoader.Value
            .GetItems(productContent.GetVariants(_relationRepository.Value), productContent.Language)
            .OfType<VariationContent>();
    }

    public static IEnumerable<T> GetAllVariants<T>(this ContentReference contentLink) where T : VariationContent
    {
        switch (_referenceConverter.Value.GetContentType(contentLink))
        {
            case CatalogContentType.CatalogNode:
                return _contentLoader.Value.GetChildren<T>(contentLink,
                    [LanguageLoaderOption.FallbackWithMaster()]);
            case CatalogContentType.CatalogEntry:
                var entryContent = _contentLoader.Value.Get<EntryContentBase>(contentLink);

                if (entryContent is ProductContent p)
                {
                    return p.GetVariants().Select(c => _contentLoader.Value.Get<T>(c));
                }

                if (entryContent is T entry)
                {
                    return [entry];
                }

                break;
        }

        return Enumerable.Empty<T>();
    }

    public static EntryContentBase? GetEntryContentBase(this ILineItem lineItem) => GetEntryContent<EntryContentBase>(lineItem.Code);

    public static EntryContentBase? GetEntryContentBase(this LineItemReportingModel lineItem) => GetEntryContent<EntryContentBase>(lineItem.LineItemCode);

    public static T? GetEntryContent<T>(this ILineItem lineItem) where T : EntryContentBase => GetEntryContent<T>(lineItem.Code);

    private static bool ValidateCorrectType<T>(ContentReference contentLink,
        IContentLoader contentLoader)
        where T : IContentMedia => typeof(T) == typeof(IContentMedia) || (!ContentReference.IsNullOrEmpty(contentLink) && contentLoader.TryGet(contentLink, out T _));
}
