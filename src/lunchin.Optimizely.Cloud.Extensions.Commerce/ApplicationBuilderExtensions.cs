using Microsoft.AspNetCore.Builder;

namespace lunchin.Optimizely.Cloud.Extensions.Commerce;

public static class ApplicationBuilderExtensions
{
    public static void UselunchinOptimizelyCommerceCloudExtensions(this IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.ApplicationServices.GetInstance<IUniqueCouponService>()?.InitializeDatabase();
    }
}
