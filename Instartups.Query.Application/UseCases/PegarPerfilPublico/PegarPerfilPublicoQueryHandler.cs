using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.Interfaces.Repositories;
using Instartups.Query.Application.Exceptions;

namespace Instartups.Query.Application.UseCases.PegarPerfilPublico;

public class PegarPerfilPublicoQueryHandler(
        IPerfilRepository perfilRepository
    ) : IQueryHandler<PegarPerfilPublicoQuery, PegarPerfilPublicoQueryResponse>
{
    public async Task<PegarPerfilPublicoQueryResponse> Handle(PegarPerfilPublicoQuery query, CancellationToken ct)
    {
        var perfil = await perfilRepository.PegarPerfilPublicoAsync(query.PerfilId, query.UserId, ct)
            ?? throw new PerfilNaoEncontradoException();

        return PegarPerfilPublicoQueryResponse.FromModel(perfil);
    }
}
