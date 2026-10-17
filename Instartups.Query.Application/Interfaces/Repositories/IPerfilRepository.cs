using Instartups.Query.Application.Common.Models;

namespace Instartups.Query.Application.Interfaces.Repositories;

public interface IPerfilRepository
{
    public Task<PerfilModel?> PegarMeuPerfilAsync(Guid usuarioId, CancellationToken ct);
}
