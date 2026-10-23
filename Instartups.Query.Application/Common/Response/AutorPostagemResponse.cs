using Instartups.Query.Application.Common.Models;
using Instartups.Query.Domain.Enums;
using Instartups.Query.Domain.Enums.Mapear;

namespace Instartups.Query.Application.Common.Response;

public sealed record AutorPostagemResponse(
    Guid Id,
    string Nome,
    string Tipo,
    string? ImagemPerfilUrl,
    string? Setor,
    double DistanciaKm,
    bool EhMeu
)
{
    public static AutorPostagemResponse FromModel(PostagemModel postagem)
    {
        return new AutorPostagemResponse(
            postagem.AutorId,
            postagem.AutorNome,
            ((TiposPerfisEnum)postagem.AutorTipoPerfil).Map(),
            postagem.AutorImagemPerfilUrl,
            postagem.AutorSetor is { } setor ? ((SetorStartupEnum)setor).Map() : null,
            postagem.AutorDistanciaKm,
            postagem.EhMeu
        );
    }
}
