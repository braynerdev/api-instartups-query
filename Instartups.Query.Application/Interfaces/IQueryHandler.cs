namespace Instartups.Query.Application.Interfaces;

public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery
{
    public Task<TResponse> Handle(TQuery query, CancellationToken ct);
}

public interface IQueryHandler<TResponse>
{
    public Task<TResponse> Handle(CancellationToken ct);
}
