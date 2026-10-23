using Instartups.Query.Application.Interfaces;

namespace Instartups.Query.Application.UseCases.PegarPerfilPublico;

public sealed record PegarPerfilPublicoQuery(
    Guid UserId,
    Guid PerfilId
) : IQuery;
