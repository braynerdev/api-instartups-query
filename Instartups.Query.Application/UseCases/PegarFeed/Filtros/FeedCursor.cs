namespace Instartups.Query.Application.UseCases.PegarFeed.Filtros;

public sealed record FeedCursor(
    DateTimeOffset DataCriacao,
    Guid Id
);
