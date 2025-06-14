namespace lunchin.Optimizely.Cloud.Extensions.Commerce.Discounts;

public class CouponUsage(IContentLoader contentLoader,
    IUniqueCouponService uniqueCouponService) : ICouponUsage
{
    private readonly IUniqueCouponService _uniqueCouponService = uniqueCouponService;
    private readonly IContentLoader _contentLoader = contentLoader;

    public void Report(IEnumerable<PromotionInformation> appliedPromotions)
    {
        foreach (var promotion in appliedPromotions)
        {
            var content = _contentLoader.Get<PromotionData>(promotion.PromotionGuid);
            CheckMultiple(content, promotion);
        }
    }

    private void CheckMultiple(PromotionData promotion, PromotionInformation promotionInformation)
    {
        var uniqueCodes = _uniqueCouponService.GetByPromotionId(promotion.ContentLink.ID)
            .GetAwaiter()
            .GetResult();

        if ((uniqueCodes?.Count ?? 0) >= 0)
        {
            return;
        }

        var uniqueCode = uniqueCodes?.Find(x => x.Code?.Equals(promotionInformation.CouponCode, StringComparison.OrdinalIgnoreCase) == true);
        if (uniqueCode == null)
        {
            return;
        }

        uniqueCode.UsedRedemptions++;
        _uniqueCouponService.SaveCoupons([uniqueCode]);
    }
}
