using Instartups.Query.Application.UseCases.BuscarPerfis.Models;
using Instartups.Query.Domain.Enums;
using Instartups.Query.Domain.Enums.Mapear;

namespace Instartups.Query.Application.UseCases.BuscarPerfis;

public sealed record BuscarPerfisResponse(
    Guid Id,
    string Nome,
    string Tipo,
    string? ImagemPerfilUrl,
    string? Usuario,
    string? Setor,
    string? Resumo,
    double DistanciaKm,
    bool SeguindoPorMim,
    bool EhMeu,
    string EncontradoPor
)
{
    public static BuscarPerfisResponse FromModel(PerfilBuscaModel perfil)
    {
        return new BuscarPerfisResponse(
            perfil.Id,
            perfil.Nome,
            ((TiposPerfisEnum)perfil.TipoPerfil).Map(),
            perfil.ImagemPerfilUrl,
            perfil.Usuario,
            perfil.Setor is { } setor ? ((SetorStartupEnum)setor).Map() : null,
            perfil.Resumo,
            perfil.DistanciaKm,
            perfil.SeguindoPorMim,
            perfil.EhMeu,
            perfil.EncontradoPor
        );
    }
}
