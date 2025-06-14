namespace lunchin.Optimizely.Cloud.Extensions.Commerce.Discounts;

public class PromotionCouponsViewModel
{
    public PromotionCouponsViewModel()
    {
        Coupons = [];
        PagingInfo = new PagingInfo();
    }

    public PromotionData? Promotion { get; set; }

    public PagingInfo PagingInfo { get; set; }

    public List<UniqueCoupon> Coupons { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    [Display(Name = "Valid From")]
    public DateTime ValidFrom { get; set; }

    public DateTime? Expiration { get; set; }

    [Required]
    [Display(Name = "Max Redemptions")]
    public int MaxRedemptions { get; set; }

    public int PromotionId { get; set; }
}
