using Instartups.Query.Domain.Enums;
using Instartups.Query.Domain.Enums.Mapear;

namespace Instartups.Query.Application.UseCases.PegarSetores;

public sealed record PegarSetoresResponse(
    string Codigo,
    string Rotulo
)
{
    public static PegarSetoresResponse FromEnum(SetorStartupEnum setor)
    {
        return new PegarSetoresResponse(setor.Map(), setor.Rotulo());
    }
}
