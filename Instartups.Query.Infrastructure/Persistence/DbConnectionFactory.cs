using System.Data.Common;
using Instartups.Query.Application.Interfaces;
using Npgsql;

namespace Instartups.Query.Infrastructure.Persistence;

public class DbConnectionFactory(
    NpgsqlDataSource _dataSource
) : IDbConnectionFactory
{
    public async Task<DbConnection> CreateConnectionAsync(CancellationToken ct)
    {
        return await _dataSource.OpenConnectionAsync(ct);
    }
}
