using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Instartups.Query.Infrastructure.Persistence;

public static class DatabaseConfig
{
    public static IServiceCollection AddPostgresConf(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var ConnectionString = configuration.GetConnectionString("PostgresConnection")
                               ?? throw new InvalidOperationException("Connection string 'PostgresConnection' não configurada.");

        DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddSingleton(new NpgsqlDataSourceBuilder(ConnectionString).Build());

        return services;
    }
}
