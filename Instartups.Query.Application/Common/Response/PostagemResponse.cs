using Instartups.Query.Application.Common.Models;

namespace Instartups.Query.Application.Common.Response;

public sealed record PostagemResponse(
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
    public static PostagemResponse FromModel(PostagemModel postagem)
    {
        return new PostagemResponse(
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
