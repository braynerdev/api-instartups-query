namespace Instartups.Query.Application.Common.Response;

public sealed record PaginaResponse<T>(
    IReadOnlyList<T> Itens,
    string? ProximoCursor,
    bool TemMais
);
