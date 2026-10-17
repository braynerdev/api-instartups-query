using Instartups.Query.Application.Interfaces;

namespace Instartups.Query.Application.UseCases.BuscarPerfis;

public sealed record BuscarPerfisQuery(
    Guid UserId,
    string Termo,
    string Tipo,
    string? Cursor,
    int Limite
) : IPaginadaQuery;
