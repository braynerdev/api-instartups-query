using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.Interfaces.Repositories;
using Instartups.Query.Application.Exceptions;

namespace Instartups.Query.Application.UseCases.PegarMeuPerfil;

public class PegarMeuPerfilQueryHandler(
        IPerfilRepository perfilRepository
    ) : IQueryHandler<PegarMeuPerfilQuery, PegarMeuPerfilResponse>
{
    public async Task<PegarMeuPerfilResponse> Handle(PegarMeuPerfilQuery query, CancellationToken ct)
    {
        var perfil = await perfilRepository.PegarMeuPerfilAsync(query.UserId, ct)
            ?? throw new PerfilNaoEncontradoException();

        return PegarMeuPerfilResponse.FromModel(perfil);
    }
}
