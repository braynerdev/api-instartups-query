using System.Buffers.Text;
using System.Text.Json;
using Instartups.Query.Application.Common.Response;
using Instartups.Query.Application.Exceptions;
using Instartups.Query.Application.Interfaces;

namespace Instartups.Query.Infrastructure.Service;

public class PaginadorService : IPaginadorService
{
    public TCursor? LerCursor<TCursor>(string? cursor) where TCursor : class
    {
        return string.IsNullOrWhiteSpace(cursor)
            ? null
            : Decodificar<TCursor>(cursor);
    }

    public int LimiteBusca(int limite) => limite + 1;

    public PaginaResponse<TResponse> MontarPagina<TModel, TResponse, TCursor>(
        IReadOnlyList<TModel> buscados,
        int limite,
        Func<TModel, TCursor> gerarCursor,
        Func<TModel, TResponse> mapear)
    {
        var temMais = buscados.Count > limite;
        var itens = temMais ? buscados.Take(limite).ToList() : buscados;

        var proximoCursor = temMais
            ? Codificar(gerarCursor(itens[^1]))
            : null;

        return new PaginaResponse<TResponse>(
            itens.Select(mapear).ToList(),
            proximoCursor,
            temMais
        );
    }

    private static string Codificar<TCursor>(TCursor cursor)
    {
        return Base64Url.EncodeToString(JsonSerializer.SerializeToUtf8Bytes(cursor));
    }

    private static TCursor Decodificar<TCursor>(string cursor)
    {
        try
        {
            return JsonSerializer.Deserialize<TCursor>(Base64Url.DecodeFromChars(cursor))
                ?? throw new CursorInvalidoException();
        }
        catch (Exception ex) when (ex is not CursorInvalidoException)
        {
            throw new CursorInvalidoException();
        }
    }
}
