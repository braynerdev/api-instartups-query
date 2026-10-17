using Instartups.Query.Application.UseCases.PegarFeed.Models;
using Instartups.Query.Application.UseCases.PegarFeed.Responses;

namespace Instartups.Query.Application.UseCases.PegarFeed;

public sealed record PegarFeedQueryResponse(
    Guid Id,
    string Descricao,
    DateTimeOffset DataCriacao,
    long TotalCurtidas,
    bool CurtidaPorMim,
    bool Ativo,
    IReadOnlyList<MidiaPostagemResponse> Midias,
    AutorPostagemResponse Autor
)
{
    public static PegarFeedQueryResponse FromModel(PostagemFeedModel postagem)
    {
        return new PegarFeedQueryResponse(
            postagem.Id,
            postagem.Descricao,
            postagem.DataCriacao,
            postagem.TotalCurtidas,
            postagem.CurtidaPorMim,
            postagem.Ativo,
            postagem.Midias.Select(MidiaPostagemResponse.FromModel).ToList(),
            AutorPostagemResponse.FromModel(postagem)
        );
    }
}
