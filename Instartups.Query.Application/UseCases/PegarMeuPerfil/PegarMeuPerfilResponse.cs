using Instartups.Query.Application.Common.Response;
using Instartups.Query.Domain.Models;
using Instartups.Query.Domain.Enums;
using Instartups.Query.Domain.Enums.Mapear;

namespace Instartups.Query.Application.UseCases.PegarMeuPerfil;

public sealed record PegarMeuPerfilResponse(
    Guid Id,
    string Tipo,
    string Nome,
    string? Usuario,
    string? ImagemPerfilUrl,
    string? ImagemFundoUrl,
    CoordenadaResponse Coordenada,
    int TotalSeguidores,
    int TotalSeguindo,
    int TotalCurtidas,
    bool Ativo,
    DateTime DataCriacao,
    StartupResponse? Startup,
    InvestidorResponse? Investidor
)
{
    public static PegarMeuPerfilResponse FromModel(PerfilModel perfil)
    {
        var tipo = (TiposPerfisEnum)perfil.TipoPerfil;

        return new PegarMeuPerfilResponse(
            perfil.Id,
            tipo.Map(),
            perfil.Nome,
            perfil.Usuario,
            perfil.ImagemPerfilUrl,
            perfil.ImagemFundoUrl,
            new CoordenadaResponse(perfil.Latitude, perfil.Longitude),
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
