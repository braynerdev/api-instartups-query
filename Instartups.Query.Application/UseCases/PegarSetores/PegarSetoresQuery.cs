using Instartups.Query.Application.Interfaces;

namespace Instartups.Query.Application.UseCases.PegarSetores;

public sealed record PegarSetoresQuery(
    string? Busca
) : IQuery;
