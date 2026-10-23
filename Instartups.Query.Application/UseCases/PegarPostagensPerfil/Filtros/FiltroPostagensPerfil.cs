namespace Instartups.Query.Application.UseCases.PegarPostagensPerfil.Filtros;

public sealed record FiltroPostagensPerfil(
    Guid MeuPerfilId,
    Guid PerfilId,
    DateTimeOffset? CursorData,
    Guid? CursorId,
    int Limite
);
