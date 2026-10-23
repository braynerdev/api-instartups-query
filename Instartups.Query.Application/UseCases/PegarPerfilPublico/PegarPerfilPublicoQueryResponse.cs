using Instartups.Query.Application.Common.Response;
using Instartups.Query.Domain.Enums;
using Instartups.Query.Domain.Enums.Mapear;

namespace Instartups.Query.Application.UseCases.PegarPerfilPublico;

public sealed record PegarPerfilPublicoQueryResponse(
    Guid Id,
    string Tipo,
    string Nome,
    string? Usuario,
    string? ImagemPerfilUrl,
    string? ImagemFundoUrl,
    double? DistanciaKm,
    bool SeguindoPorMim,
    int TotalSeguidores,
    int TotalSeguindo,
    int TotalCurtidas,
    bool Ativo,
    DateTimeOffset DataCriacao,
    StartupResponse? Startup,
    InvestidorResponse? Investidor
)
{
    public static PegarPerfilPublicoQueryResponse FromModel(PerfilPublicoModel perfil)
    {
        var tipo = (TiposPerfisEnum)perfil.TipoPerfil;

        return new PegarPerfilPublicoQueryResponse(
            perfil.Id,
            tipo.Map(),
            perfil.Nome,
            perfil.Usuario,
            perfil.ImagemPerfilUrl,
            perfil.ImagemFundoUrl,
            perfil.DistanciaKm,
            perfil.SeguindoPorMim,
            perfil.TotalSeguidores,
            perfil.TotalSeguindo,
            perfil.TotalCurtidas,
            perfil.Ativo,
            perfil.DataCriacao,
            tipo == TiposPerfisEnum.Startup ? StartupResponse.FromModel(perfil) : null,
            tipo == TiposPerfisEnum.Investidor ? InvestidorResponse.FromModel(perfil) : null
        );
    }
}
