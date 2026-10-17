namespace Instartups.Query.Application.Common.Filtros;

public sealed record PostagemCursor(
    DateTimeOffset DataCriacao,
    Guid Id
);
