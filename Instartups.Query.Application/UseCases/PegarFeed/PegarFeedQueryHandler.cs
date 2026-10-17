using Instartups.Query.Application.Common.Response;
using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.Interfaces.Repositories;
using Instartups.Query.Application.Exceptions;
using Instartups.Query.Application.UseCases.PegarFeed.Filtros;
using Instartups.Query.Domain.Enums;
using Instartups.Query.Domain.Enums.Mapear;

namespace Instartups.Query.Application.UseCases.PegarFeed;

public class PegarFeedQueryHandler(
        IPerfilRepository perfilRepository,
        IPostagemRepository postagemRepository,
        IPaginadorService paginador
    ) : IQueryHandler<PegarFeedQuery, PaginaResponse<PegarFeedQueryResponse>>
{
    public async Task<PaginaResponse<PegarFeedQueryResponse>> Handle(PegarFeedQuery query, CancellationToken ct)
    {
        var cursor = paginador.LerCursor<FeedCursor>(query.Cursor);

        var meuPerfilId = await perfilRepository.PegarMeuPerfilIdAsync(query.UserId, ct)
            ?? throw new PerfilNaoEncontradoException();

        var tipo = query.Tipo.MapTiposPerfis();

        var setores = tipo == TiposPerfisEnum.Startup
            ? query.Setores.Select(setor => (int)setor.MapSetorStartup()).Distinct().ToArray()
            : [];

        var filtro = new FiltroFeed(
            meuPerfilId,
            tipo == 0 ? null : (int)tipo,
            query.RaioKm,
            setores,
            cursor?.DataCriacao,
            cursor?.Id,
            paginador.LimiteBusca(query.Limite)
        );

        var postagens = await postagemRepository.PegarFeedAsync(filtro, ct);

        return paginador.MontarPagina(
            postagens,
            query.Limite,
            postagem => new FeedCursor(postagem.DataCriacao, postagem.Id),
            PegarFeedQueryResponse.FromModel
        );
    }
}
