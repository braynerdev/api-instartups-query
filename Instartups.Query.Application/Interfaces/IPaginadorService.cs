using Instartups.Query.Application.Common.Response;

namespace Instartups.Query.Application.Interfaces;

public interface IPaginadorService
{
    public TCursor? LerCursor<TCursor>(string? cursor) where TCursor : class;

    public int LimiteBusca(int limite);

    public PaginaResponse<TResponse> MontarPagina<TModel, TResponse, TCursor>(
        IReadOnlyList<TModel> buscados,
        int limite,
        Func<TModel, TCursor> gerarCursor,
        Func<TModel, TResponse> mapear);
}
