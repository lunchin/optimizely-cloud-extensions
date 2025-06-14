using lunchin.Optimizely.Cloud.Extensions.Commerce.Discounts;

namespace lunchin.Optimizely.Cloud.Extensions.Commerce;

public static class ServiceCollectionExtensions
{
    private const string ModuleName = "lunchin.Optimizely.Cloud.Extensions.Commerce";

    public static IServiceCollection AddlunchinOptimizelyCommerceCloudExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddApplicationPart(typeof(MultiCouponsController).Assembly);

        services.AddSingleton<IUniqueCouponService, UniqueCouponService>();
        services.AddSingleton<ICouponFilter, CouponFilter>();
        services.AddSingleton<ICouponUsage, CouponUsage>();
        services.Configure<CommerceExtensionsOptions>(configuration.GetSection(CommerceExtensionsOptions.Path));
        services.Configure<AuthorizationOptions>(x => x.TryAddPolicy("loce:policy", p => p.RequireRole(Roles.Administrators, Roles.CmsAdmins)));
        services.Configure<ProtectedModuleOptions>(moduleOptions =>
        {
            if (!moduleOptions.Items.Any(item => item.Name.Equals(ModuleName, StringComparison.OrdinalIgnoreCase)))
            {
                moduleOptions.Items.Add(new ModuleDetails { Name = ModuleName });
            }
        });
        return services;
    }
}
