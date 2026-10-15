using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.Interfaces.Repositories;
using Instartups.Query.Infrastructure.Persistence;
using Instartups.Query.Infrastructure.Persistence.Repositories;
using Instartups.Query.Infrastructure.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Instartups.Query.Infrastructure.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresConf(configuration);
        services.AddHttpContextAccessor();

        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddSingleton<LerArquivosSQL>();
        services.AddScoped<IPegarContextoRequisicao, PegarContextoRequisicao>();
        services.AddScoped<IPerfilRepository, PerfilRepository>();
        return services;
    }
}
