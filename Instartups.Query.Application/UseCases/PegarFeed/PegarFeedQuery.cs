using Instartups.Query.Application.Interfaces;

namespace Instartups.Query.Application.UseCases.PegarFeed;

public sealed record PegarFeedQuery(
    Guid UserId,
    string Tipo,
    double? RaioKm,
    string[] Setores,
    string? Cursor,
    int Limite
) : IPaginadaQuery;
