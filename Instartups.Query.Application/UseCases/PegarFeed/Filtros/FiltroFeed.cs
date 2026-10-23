namespace Instartups.Query.Application.UseCases.PegarFeed.Filtros;

public sealed record FiltroFeed(
    Guid MeuPerfilId,
    int? TipoPerfil,
    double? RaioKm,
    int[] Setores,
    DateTimeOffset? CursorData,
    Guid? CursorId,
    int Limite
);
