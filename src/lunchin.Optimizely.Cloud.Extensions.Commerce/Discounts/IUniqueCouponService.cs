using System.Threading.Tasks;

namespace lunchin.Optimizely.Cloud.Extensions.Commerce.Discounts;

public interface IUniqueCouponService
{
    Task<bool> SaveCoupons(List<UniqueCoupon> coupons);

    Task<bool> DeleteById(long id);

    Task<bool> DeleteByPromotionId(int id);

    Task<List<UniqueCoupon>?> GetByPromotionId(int id);

    Task<UniqueCoupon?> GetById(long id);

    string GenerateCoupon();

    Task<bool> DeleteExpiredCoupons();

    Task InitializeDatabase();
}
