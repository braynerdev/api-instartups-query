namespace Instartups.Query.Api.Configurations;

public static class LowerCasesConfig
{
    public static IServiceCollection AddLowerCaseConfig(this IServiceCollection services)
    {
        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });
        return services;
    }
}
