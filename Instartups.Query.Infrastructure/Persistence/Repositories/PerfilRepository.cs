using Dapper;
using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.Interfaces.Repositories;
using Instartups.Query.Application.UseCases.PegarMeuPerfil;
using Instartups.Query.Application.UseCases.PegarPerfilPublico;
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

    public async Task<PerfilPublicoModel?> PegarPerfilPublicoAsync(Guid perfilId, Guid usuarioId, CancellationToken ct)
    {
        await using var connection = await factory.CreateConnectionAsync(ct);
        var sql = await lerArquivosSQL.LerArquivoAsync("PegarPerfilPublicoSQL.sql", ct);

        return await connection.QuerySingleOrDefaultAsync<PerfilPublicoModel>(
            new CommandDefinition(sql, new { PerfilId = perfilId, UsuarioId = usuarioId }, cancellationToken: ct)
        );
    }
}
