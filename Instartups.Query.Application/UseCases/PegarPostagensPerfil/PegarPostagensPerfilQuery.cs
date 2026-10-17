using Instartups.Query.Application.Interfaces;

namespace Instartups.Query.Application.UseCases.PegarPostagensPerfil;

public sealed record PegarPostagensPerfilQuery(
    Guid UserId,
    Guid PerfilId,
    string? Cursor,
    int Limite
) : IPaginadaQuery;
