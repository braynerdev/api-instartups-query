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
        services.AddSingleton<LerArquivosSQLService>();
        services.AddSingleton<IPaginadorService, PaginadorService>();
        services.AddScoped<IPegarContextoRequisicaoService, PegarContextoRequisicaoService>();
        services.AddScoped<IPerfilRepository, PerfilRepository>();
        services.AddScoped<IPostagemRepository, PostagemRepository>();
        return services;
    }
}
