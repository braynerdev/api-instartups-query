using System.Data.Common;
using Dapper;
using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.Interfaces.Repositories;
using Instartups.Query.Application.UseCases.PegarFeed.Filtros;
using Instartups.Query.Application.UseCases.PegarFeed.Models;
using Instartups.Query.Infrastructure.Service;

namespace Instartups.Query.Infrastructure.Persistence.Repositories;

public class PostagemRepository(
        IDbConnectionFactory factory,
        LerArquivosSQLService lerArquivosSQL
    ) : IPostagemRepository
{
    public async Task<IReadOnlyList<PostagemFeedModel>> PegarFeedAsync(FiltroFeed filtro, CancellationToken ct)
    {
        await using var connection = await factory.CreateConnectionAsync(ct);
        var sql = await lerArquivosSQL.LerArquivoAsync("PegarFeedSQL.sql", ct);

        var postagens = (await connection.QueryAsync<PostagemFeedModel>(
            new CommandDefinition(sql, filtro, cancellationToken: ct)
        )).AsList();

        await PreencherMidiasAsync(connection, postagens, ct);

        return postagens;
    }

    private async Task PreencherMidiasAsync(DbConnection connection, IReadOnlyList<PostagemFeedModel> postagens, CancellationToken ct)
    {
        if (postagens.Count == 0)
            return;

        var sql = await lerArquivosSQL.LerArquivoAsync("PegarMidiasPostagensSQL.sql", ct);
        var postagemIds = postagens.Select(p => p.Id).ToArray();

        var midias = await connection.QueryAsync<MidiaPostagemModel>(
            new CommandDefinition(sql, new { PostagemIds = postagemIds }, cancellationToken: ct)
        );

        var midiasPorPostagem = midias.ToLookup(m => m.PostagemId);

        foreach (var postagem in postagens)
            postagem.Midias.AddRange(midiasPorPostagem[postagem.Id]);
    }
}
