using Instartups.Query.Application.Common.Constants;
using Instartups.Query.Application.Common.Extensions;
using Instartups.Query.Application.Interfaces;
using Instartups.Query.Domain.Enums;

namespace Instartups.Query.Application.UseCases.PegarSetores;

public class PegarSetoresQueryHandler : IQueryHandler<PegarSetoresQuery, IReadOnlyList<PegarSetoresResponse>>
{
    private static readonly IReadOnlyList<(PegarSetoresResponse Setor, string RotuloNormalizado)> Setores =
        Enum.GetValues<SetorStartupEnum>()
            .Select(PegarSetoresResponse.FromEnum)
            .Select(setor => (Setor: setor, RotuloNormalizado: setor.Rotulo.NormalizarParaBusca()))
            .OrderBy(setor => setor.RotuloNormalizado, StringComparer.Ordinal)
            .ToList();

    public Task<IReadOnlyList<PegarSetoresResponse>> Handle(PegarSetoresQuery query, CancellationToken ct)
    {
        var busca = string.IsNullOrWhiteSpace(query.Busca)
            ? null
            : query.Busca.NormalizarParaBusca();

        IReadOnlyList<PegarSetoresResponse> setores = Setores
            .Where(setor => busca is null || setor.RotuloNormalizado.Contains(busca, StringComparison.Ordinal))
            .Take(PaginacaoConst.LimiteMaximo)
            .Select(setor => setor.Setor)
            .ToList();

        return Task.FromResult(setores);
    }
}
