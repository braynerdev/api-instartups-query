using Instartups.Query.Application.Common.Models;
using Instartups.Query.Domain.Enums;
using Instartups.Query.Domain.Enums.Mapear;

namespace Instartups.Query.Application.Common.Response;

public sealed record StartupResponse(
    string Pitch,
    DateOnly DataFundacao,
    int TamanhoEquipe,
    decimal ValorBuscado,
    string Setor
)
{
    public static StartupResponse FromModel(PerfilModel perfil)
    {
        return new StartupResponse(
            perfil.Pitch!,
            DateOnly.FromDateTime(perfil.DataFundacao!.Value),
            perfil.TamanhoEquipe!.Value,
            perfil.ValorBuscado!.Value,
            ((SetorStartupEnum)perfil.Setor!.Value).Map()
        );
    }
}
