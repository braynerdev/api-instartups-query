using Dapper;
using Instartups.Query.Application.Common.Models;
using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.Interfaces.Repositories;
using Instartups.Query.Infrastructure.Service;

namespace Instartups.Query.Infrastructure.Persistence.Repositories;

public class PerfilRepository(
        IDbConnectionFactory factory,
        LerArquivosSQL lerArquivosSQL
    ) : IPerfilRepository
{
    public async Task<PerfilModel?> PegarMeuPerfilAsync(Guid usuarioId, CancellationToken ct)
    {
        await using var connection = await factory.CreateConnectionAsync(ct);
        var sql = await lerArquivosSQL.LerArquivoAsync("PegarMeuPerfilSQL.sql", ct);

        return await connection.QuerySingleOrDefaultAsync<PerfilModel>(
            new CommandDefinition(sql, new { UsuarioId = usuarioId }, cancellationToken: ct)
        );
    }
}
