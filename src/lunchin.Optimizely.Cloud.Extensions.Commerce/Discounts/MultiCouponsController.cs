using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Options;

namespace lunchin.Optimizely.Cloud.Extensions.Commerce.Discounts;

[Authorize(Policy = Constants.lunchinPolicy)]
public class MultiCouponsController(IContentLoader contentLoader,
    IUniqueCouponService couponService,
    IOptions<AntiforgeryOptions> options) : Controller
{
    private const string BaseRoute = "/api/multicoupons/";
    private readonly IContentLoader _contentLoader = contentLoader;
    private readonly IUniqueCouponService _couponService = couponService;
    private readonly AntiforgeryOptions _antiforgeryOptions = options.Value;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return await Task.FromResult(View(new MultiCouponsViewModel
        {
            AntiforgeryOptions = _antiforgeryOptions,
        }));
    }

    [HttpGet]
    [Route($"{BaseRoute}promotions", Name = "promotionsGet")]
    public async Task<IActionResult> Search(int page = 1, int pageSize = 25)
    {
        var promotions = GetPromotions(_contentLoader.GetDescendents(GetCampaignRoot()))
            .Select(x => new
                {
                    id = x.ContentLink.ID,
                    name = x.Name,
                    discountType = x.DiscountType.ToString(),
                    isActive = x.IsActive,
                })
            .ToList();

        return await Task.FromResult(Json(new
        {
            promotions = promotions
                .Skip((page - 1) * pageSize)
                .Take(pageSize),
            total = promotions.Count
        }));
    }

    [HttpGet]
    [Route($"{BaseRoute}coupons", Name = "editPromotionCoupons")]
    public async Task<IActionResult> EditPromotionCoupons(int id)
    {
        var promotion = _contentLoader.Get<PromotionData>(new ContentReference(id));
        var coupons = await _couponService.GetByPromotionId(id);

        return Json(new
        {
            Coupons = coupons ?? [],
            PromotionName = promotion.Name,
            PromotionId = promotion.ContentLink.ID,
            MaxRedemptions = 1
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route($"{BaseRoute}updateOrDeleteCoupon", Name = "updateOrDeleteCoupon")]
    public async Task<IActionResult> UpdateOrDeleteCoupon(UniqueCoupon model)
    {
        if (model?.ActionType?.Equals("update", StringComparison.Ordinal) ?? false)
        {
            var updated = false;
            var coupon = await _couponService.GetById(model.Id);

            if (coupon != null)
            {
                coupon.Code = model.Code;
                coupon.Expiration = model.Expiration;
                coupon.MaxRedemptions = model.MaxRedemptions;
                coupon.UsedRedemptions = model.UsedRedemptions;
                coupon.ValidFrom = model.ValidFrom;
                updated = await _couponService.SaveCoupons([coupon]);
            }

            return updated ? Ok() : BadRequest("Could not save coupons");
        }
        else
        {
            var deleted = await _couponService.DeleteById(model?.Id ?? 0);
            return deleted ? Ok() : BadRequest("Could not delete coupons");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route($"{BaseRoute}generateCoupon", Name = "generateCoupon")]
    public async Task<IActionResult> Generate([FromBody] PromotionCouponsViewModel model)
    {
        var couponRecords = new List<UniqueCoupon>();
        for (var i = 0; i < model.Quantity; i++)
        {
            couponRecords.Add(new UniqueCoupon
            {
                Code = _couponService.GenerateCoupon(),
                Created = DateTime.UtcNow,
                Expiration = model.Expiration,
                MaxRedemptions = model.MaxRedemptions,
                PromotionId = model.PromotionId,
                UsedRedemptions = 0,
                ValidFrom = model.ValidFrom
            });
        }

        await _couponService.SaveCoupons(couponRecords);
        return Json(new { id = model.PromotionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route($"{BaseRoute}deleteAllCoupons", Name = "deleteAllCoupons")]
    public async Task<IActionResult> DeleteAll([FromBody] PromotionCouponsViewModel model)
    {
        await _couponService.DeleteByPromotionId(model.PromotionId);
        return Json( new { id = model.PromotionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route($"{BaseRoute}downloadCoupons", Name = "downloadCoupons")]
    public async Task<FileResult> Download([FromBody] PromotionCouponsViewModel model)
    {
        var coupons = await _couponService.GetByPromotionId(model.PromotionId) ?? throw new Exception($"Cannot find promotion with id {model.PromotionId}");
        var sb = new StringBuilder();
        sb.Append("PromotionId,Code,ValidFrom,Expiration,CustomerId,MaxRedemptions,UsedRedemptions");
        sb.Append("\r\n");
        for (var i = 0; i < coupons.Count; i++)
        {
            sb.Append(coupons[i].PromotionId)
                .Append(',')
                .Append(coupons[i].Code)
                .Append(',')
                .Append(coupons[i].ValidFrom)
                .Append(',')
                .Append(coupons[i].Expiration)
                .Append(',')
                .Append(coupons[i].CustomerId)
                .Append(',')
                .Append(coupons[i].MaxRedemptions)
                .Append(',')
                .Append(coupons[i].UsedRedemptions);
            sb.Append("\r\n");
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"{model.PromotionId}.csv");
    }

    private ContentReference GetCampaignRoot()
    {
        return _contentLoader.GetChildren<SalesCampaignFolder>(ContentReference.RootPage)
            .FirstOrDefault()?.ContentLink ?? ContentReference.EmptyReference;
    }

    private List<PromotionData> GetPromotions(IEnumerable<ContentReference> references)
    {
        return _contentLoader.GetItems(references, ContentLanguage.PreferredCulture)
            .OfType<PromotionData>()
            .ToList();
    }
}
