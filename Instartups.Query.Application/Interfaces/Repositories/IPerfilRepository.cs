using Instartups.Query.Application.UseCases.PegarMeuPerfil;
using Instartups.Query.Application.UseCases.PegarPerfilPublico;

namespace Instartups.Query.Application.Interfaces.Repositories;

public interface IPerfilRepository
{
    public Task<PerfilModel?> PegarMeuPerfilAsync(Guid usuarioId, CancellationToken ct);
    public Task<PerfilPublicoModel?> PegarPerfilPublicoAsync(Guid perfilId, Guid usuarioId, CancellationToken ct);
    public Task<Guid?> PegarMeuPerfilIdAsync(Guid usuarioId, CancellationToken ct);
    public Task<bool> PerfilVisivelAsync(Guid perfilId, Guid meuPerfilId, CancellationToken ct);
}
