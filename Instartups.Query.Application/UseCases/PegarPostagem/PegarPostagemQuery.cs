using Instartups.Query.Application.Interfaces;

namespace Instartups.Query.Application.UseCases.PegarPostagem;

public sealed record PegarPostagemQuery(
    Guid UserId,
    Guid PostagemId
) : IQuery;
