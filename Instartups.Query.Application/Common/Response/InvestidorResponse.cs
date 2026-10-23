using Instartups.Query.Application.Common.Models;

namespace Instartups.Query.Application.Common.Response;

public sealed record InvestidorResponse(
    string TeseInvestimento,
    decimal TicketMinimo,
    decimal TicketMaximo
)
{
    public static InvestidorResponse FromModel(PerfilBaseModel perfil)
    {
        return new InvestidorResponse(
            perfil.TeseInvestimento!,
            perfil.TicketMinimo!.Value,
            perfil.TicketMaximo!.Value
        );
    }
}
