using Instartups.Query.Application.Common.Filtros;
using Instartups.Query.Application.Common.Response;
using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.Interfaces.Repositories;
using Instartups.Query.Application.Exceptions;
using Instartups.Query.Application.UseCases.PegarPostagensPerfil.Filtros;

namespace Instartups.Query.Application.UseCases.PegarPostagensPerfil;

public class PegarPostagensPerfilQueryHandler(
        IPerfilRepository perfilRepository,
        IPostagemRepository postagemRepository,
        IPaginadorService paginador
    ) : IQueryHandler<PegarPostagensPerfilQuery, PaginaResponse<PostagemResponse>>
{
    public async Task<PaginaResponse<PostagemResponse>> Handle(PegarPostagensPerfilQuery query, CancellationToken ct)
    {
        var cursor = paginador.LerCursor<PostagemCursor>(query.Cursor);

        var meuPerfilId = await perfilRepository.PegarMeuPerfilIdAsync(query.UserId, ct)
            ?? throw new PerfilNaoEncontradoException();

        if (!await perfilRepository.PerfilVisivelAsync(query.PerfilId, meuPerfilId, ct))
            throw new PerfilNaoEncontradoException();

        var filtro = new FiltroPostagensPerfil(
            meuPerfilId,
            query.PerfilId,
            cursor?.DataCriacao,
            cursor?.Id,
            paginador.LimiteBusca(query.Limite)
        );

        var postagens = await postagemRepository.PegarPostagensPerfilAsync(filtro, ct);

        return paginador.MontarPagina(
            postagens,
            query.Limite,
            postagem => new PostagemCursor(postagem.DataCriacao, postagem.Id),
            PostagemResponse.FromModel
        );
    }
}
