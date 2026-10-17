using Instartups.Query.Application.Common.Response;
using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.Interfaces.Repositories;
using Instartups.Query.Application.Exceptions;

namespace Instartups.Query.Application.UseCases.PegarPostagem;

public class PegarPostagemQueryHandler(
        IPerfilRepository perfilRepository,
        IPostagemRepository postagemRepository
    ) : IQueryHandler<PegarPostagemQuery, PostagemResponse>
{
    public async Task<PostagemResponse> Handle(PegarPostagemQuery query, CancellationToken ct)
    {
        var meuPerfilId = await perfilRepository.PegarMeuPerfilIdAsync(query.UserId, ct)
            ?? throw new PerfilNaoEncontradoException();

        var postagem = await postagemRepository.PegarPostagemAsync(query.PostagemId, meuPerfilId, ct)
            ?? throw new PostagemNaoEncontradaException();

        return PostagemResponse.FromModel(postagem);
    }
}
