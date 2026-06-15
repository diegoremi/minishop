using Microsoft.Extensions.Configuration;

namespace MiniShop.Infrastructure.Configuration;

public static class MiniShopFeaturesExtensions
{
    public static MiniShopFeaturesOptions GetMiniShopFeatures(
        this IConfiguration configuration)
    {
        return configuration
            .GetSection("Features")
            .Get<MiniShopFeaturesOptions>() ?? new MiniShopFeaturesOptions();
    }
}