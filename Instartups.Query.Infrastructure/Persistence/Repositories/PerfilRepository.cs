using Dapper;
using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.Interfaces.Repositories;
using Instartups.Query.Application.UseCases.BuscarPerfis.Filtros;
using Instartups.Query.Application.UseCases.BuscarPerfis.Models;
using Instartups.Query.Application.UseCases.PegarMeuPerfil;
using Instartups.Query.Application.UseCases.PegarPerfilPublico;
using Instartups.Query.Infrastructure.Service;

namespace Instartups.Query.Infrastructure.Persistence.Repositories;

public class PerfilRepository(
        IDbConnectionFactory factory,
        LerArquivosSQLService lerArquivosSQL
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

    public async Task<Guid?> PegarMeuPerfilIdAsync(Guid usuarioId, CancellationToken ct)
    {
        await using var connection = await factory.CreateConnectionAsync(ct);
        var sql = await lerArquivosSQL.LerArquivoAsync("PegarMeuPerfilIdSQL.sql", ct);

        return await connection.ExecuteScalarAsync<Guid?>(
            new CommandDefinition(sql, new { UsuarioId = usuarioId }, cancellationToken: ct)
        );
    }

    public async Task<bool> PerfilVisivelAsync(Guid perfilId, Guid meuPerfilId, CancellationToken ct)
    {
        await using var connection = await factory.CreateConnectionAsync(ct);
        var sql = await lerArquivosSQL.LerArquivoAsync("PerfilVisivelSQL.sql", ct);

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { PerfilId = perfilId, MeuPerfilId = meuPerfilId }, cancellationToken: ct)
        );
    }

    public async Task<IReadOnlyList<PerfilBuscaModel>> BuscarPerfisAsync(FiltroBuscaPerfis filtro, CancellationToken ct)
    {
        await using var connection = await factory.CreateConnectionAsync(ct);
        var sql = await lerArquivosSQL.LerArquivoAsync("BuscarPerfisSQL.sql", ct);

        var perfis = await connection.QueryAsync<PerfilBuscaModel>(
            new CommandDefinition(sql, filtro, cancellationToken: ct)
        );

        return perfis.AsList();
    }
}
