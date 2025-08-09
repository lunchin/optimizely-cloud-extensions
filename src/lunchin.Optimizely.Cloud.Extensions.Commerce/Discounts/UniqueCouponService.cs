using System.Data;
using System.Threading.Tasks;
using EPiServer.Framework.Cache;
using EPiServer.Logging;
using Mediachase.Data.Provider;
using Microsoft.Data.SqlClient;
using Powells.CouponCode;

namespace lunchin.Optimizely.Cloud.Extensions.Commerce.Discounts;

public class UniqueCouponService(IConnectionStringHandler connectionHandler, ISynchronizedObjectInstanceCache cache) : IUniqueCouponService
{
    private readonly IConnectionStringHandler _connectionHandler = connectionHandler;
    private readonly ILogger _logger = LogManager.GetLogger(typeof(UniqueCouponService));
    private readonly CouponCodeBuilder _couponCodeBuilder = new();
    private readonly ISynchronizedObjectInstanceCache _cache = cache;
    private const string _couponCachePrefix = "loce:UniqueCoupon:";
    private const string _promotionCachePrefix = "loce:Promotion:";

    private const string _idColumn = "Id";
    private const string _promotionIdColumn = "PromotionId";
    private const string _codeColumn = "Code";
    private const string _validColumn = "Valid";
    private const string _expirationColumn = "Expiration";
    private const string _customerIdColumn = "CustomerId";
    private const string _createdColumn = "Created";
    private const string _maxRedemptionsColumn = "MaxRedemptions";
    private const string _usedRedemptionsColumn = "UsedRedemptions";

    public async Task<bool> SaveCoupons(List<UniqueCoupon> coupons)
    {
        var result = await DatabaseUtilities.ExecuteNonQueryAsync(_connectionHandler.Commerce.Name,
            "lunchin_UniqueCoupons_Save",
            CommandType.StoredProcedure,
            [new SqlParameter("@Data", CreateUniqueCouponsDataTable(coupons))]);

        if (!result)
        {
            return false;
        }

        foreach (var coupon in coupons)
        {
            InvalidateCouponCache(coupon.Id);
        }
        var promoId = coupons[0].PromotionId;
        InvalidatePromotionCache(promoId);

        return true;
    }

    public async Task<bool> DeleteById(long id)
    {
        var result = await DatabaseUtilities.ExecuteNonQueryAsync(_connectionHandler.Commerce.Name,
            "lunchin_UniqueCoupons_DeleteById",
            CommandType.StoredProcedure,
            [new SqlParameter("@Id", id)]);

        if (!result)
        {
            return false;
        }

        InvalidateCouponCache(id);
        return true;
    }

    public async Task<bool> DeleteByPromotionId(int id)
    {
        var result = await DatabaseUtilities.ExecuteNonQueryAsync(_connectionHandler.Commerce.Name,
            "lunchin_UniqueCoupons_DeleteByPromotionId",
            CommandType.StoredProcedure,
            [new SqlParameter("@PromotionId", id)]);
        if (!result)
        {
            return false;
        }

        InvalidatePromotionCache(id);
        return true;
    }

    public async Task<List<UniqueCoupon>?> GetByPromotionId(int id)
    {
        try
        {
            return _cache.ReadThrough(GetPromotionCacheKey(id), () =>
            {
                return DatabaseUtilities.ExecuteReaderAsync(_connectionHandler.Commerce.Name,
                    "lunchin_UniqueCoupons_GetByPromotionId",
                    (reader) => GetUniqueCoupon(reader),
                    CommandType.StoredProcedure,
                    [new SqlParameter("@PromotionId", id)])
                .GetAwaiter()
                .GetResult();

            }, GetCacheEvictionPolicy, ReadStrategy.Wait);
        }
        catch (Exception exn)
        {
            _logger.Error(exn.Message, exn);
        }

        return await Task.FromResult<List<UniqueCoupon>?>(null);
    }

    public async Task<UniqueCoupon?> GetById(long id)
    {
        try
        {
            return _cache.ReadThrough(GetCouponCacheKey(id), () =>
            {
                var result = DatabaseUtilities.ExecuteReaderAsync(_connectionHandler.Commerce.ConnectionString,
                   "lunchin_UniqueCoupons_GetById",
                   (reader) => GetUniqueCoupon(reader),
                   CommandType.StoredProcedure,
                   [new SqlParameter("@Id", id)])
               .GetAwaiter()
               .GetResult();
                return result?.FirstOrDefault();

            }, ReadStrategy.Wait);
        }
        catch (Exception exn)
        {
            _logger.Error(exn.Message, exn);
        }

        return await Task.FromResult<UniqueCoupon?>(null);
    }

    public string GenerateCoupon()
    {
        return _couponCodeBuilder.Generate(new Options
        {
            Plaintext = "loce-Coupons"
        });
    }

    public async Task<bool> DeleteExpiredCoupons() => await DatabaseUtilities.ExecuteNonQueryAsync(_connectionHandler.Commerce.ConnectionString, "UniqueCoupons_DeleteExpiredCoupons");

    public async Task InitializeDatabase()
    {
        var couponDB = DatabaseVersion.Get();

        if (couponDB == null)
        {
            var result = await DatabaseUtilities.RunUpgradeScript(_connectionHandler.Commerce.Name,
                "lunchin.Optimizely.Cloud.Extensions.Commerce.Database.Coupons",
                Constants.CouponDatabaseVersionNumber,
                assembly: GetType().Assembly);

            if (!result)
            {
                return;
            }
            couponDB = new DatabaseVersion()
            {
                CouponDatabaseVersion = Constants.CouponDatabaseVersionNumber
            };
            couponDB.Save();
        }
        else if (!(couponDB.CouponDatabaseVersion ?? "").Equals(Constants.CouponDatabaseVersionNumber))
        {
            var result = await DatabaseUtilities.RunUpgradeScript(_connectionHandler.Commerce.Name,
                "lunchin.Optimizely.Cloud.Extensions.Commerce.Database.Coupons",
                Constants.CouponDatabaseVersionNumber,
                couponDB?.CouponDatabaseVersion ?? "0.0.0",
                GetType().Assembly);

            if (!result || couponDB == null)
            {
                return;
            }
            couponDB.CouponDatabaseVersion = Constants.CouponDatabaseVersionNumber;
            couponDB.Save();
        }
    }
    
    private static DataTable CreateUniqueCouponsDataTable(IEnumerable<UniqueCoupon> coupons)
    {
        var tblUniqueCoupon = new DataTable();
        tblUniqueCoupon.Columns.Add(new DataColumn(_idColumn, typeof(long)));
        tblUniqueCoupon.Columns.Add(_promotionIdColumn, typeof(int));
        tblUniqueCoupon.Columns.Add(_codeColumn, typeof(string));
        tblUniqueCoupon.Columns.Add(_validColumn, typeof(DateTime));
        tblUniqueCoupon.Columns.Add(_expirationColumn, typeof(DateTime));
        tblUniqueCoupon.Columns.Add(_customerIdColumn, typeof(Guid));
        tblUniqueCoupon.Columns.Add(_createdColumn, typeof(DateTime));
        tblUniqueCoupon.Columns.Add(_maxRedemptionsColumn, typeof(int));
        tblUniqueCoupon.Columns.Add(_usedRedemptionsColumn, typeof(int));

        foreach (var coupon in coupons)
        {
            var row = tblUniqueCoupon.NewRow();
            row[_idColumn] = coupon.Id;
            row[_promotionIdColumn] = coupon.PromotionId;
            row[_codeColumn] = coupon.Code;
            row[_validColumn] = coupon.ValidFrom;
            row[_expirationColumn] = coupon.Expiration ?? (object)DBNull.Value;
            row[_customerIdColumn] = coupon.CustomerId ?? (object)DBNull.Value;
            row[_createdColumn] = coupon.Created;
            row[_maxRedemptionsColumn] = coupon.MaxRedemptions;
            row[_usedRedemptionsColumn] = coupon.UsedRedemptions;
            tblUniqueCoupon.Rows.Add(row);
        }

        return tblUniqueCoupon;
    }

    private void InvalidatePromotionCache(int id)
    {
        _cache.Remove(GetPromotionCacheKey(id));
    }

    private static string GetPromotionCacheKey(int id)
    {
        return _promotionCachePrefix + id;
    }

    private void InvalidateCouponCache(long id)
    {
        _cache.Remove(GetCouponCacheKey(id));
    }

    private static string GetCouponCacheKey(long id)
    {
        return _couponCachePrefix + id;
    }

    private static CacheEvictionPolicy GetCacheEvictionPolicy(List<UniqueCoupon>? coupons)
    {
        return new CacheEvictionPolicy(TimeSpan.FromHours(1), CacheTimeoutType.Absolute, coupons?.Select(x => GetCouponCacheKey(x.Id)));
    }

    private static UniqueCoupon GetUniqueCoupon(IDataReader row)
    {
        return new UniqueCoupon
        {
            Code = row[_codeColumn].ToString(),
            Created = Convert.ToDateTime(row[_createdColumn]),
            CustomerId = row[_customerIdColumn] != DBNull.Value ? new Guid(row[_customerIdColumn]?.ToString() ?? "") : null,
            Expiration = row[_expirationColumn] != DBNull.Value
                ? Convert.ToDateTime(row[_expirationColumn].ToString())
                : null,
            Id = Convert.ToInt64(row[_idColumn]),
            MaxRedemptions = Convert.ToInt32(row[_maxRedemptionsColumn]),
            PromotionId = Convert.ToInt32(row[_promotionIdColumn]),
            UsedRedemptions = Convert.ToInt32(row[_usedRedemptionsColumn]),
            ValidFrom = Convert.ToDateTime(row[_validColumn])
        };
    }
}
