using System.Data.Common;

namespace Instartups.Query.Application.Interfaces;

public interface IDbConnectionFactory
{
    public Task<DbConnection> CreateConnectionAsync(CancellationToken ct);
}
