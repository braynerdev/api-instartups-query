using Instartups.Query.Application.Common.Response;
using Instartups.Query.Application.Exceptions;
using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.Interfaces.Repositories;
using Instartups.Query.Application.UseCases.BuscarPerfis.Filtros;
using Instartups.Query.Domain.Enums.Mapear;

namespace Instartups.Query.Application.UseCases.BuscarPerfis;

public class BuscarPerfisQueryHandler(
        IPerfilRepository perfilRepository,
        IPaginadorService paginador
    ) : IQueryHandler<BuscarPerfisQuery, PaginaResponse<BuscarPerfisResponse>>
{
    public async Task<PaginaResponse<BuscarPerfisResponse>> Handle(BuscarPerfisQuery query, CancellationToken ct)
    {
        var cursor = paginador.LerCursor<PerfilBuscaCursor>(query.Cursor);

        var meuPerfilId = await perfilRepository.PegarMeuPerfilIdAsync(query.UserId, ct)
            ?? throw new PerfilNaoEncontradoException();

        var tipo = query.Tipo.MapTiposPerfis();

        var filtro = new FiltroBuscaPerfis(
            meuPerfilId,
            query.Termo.Trim(),
            query.Termo.Trim().ToUpperInvariant(),
            tipo == 0 ? null : (int)tipo,
            cursor?.Nome,
            cursor?.Id,
            paginador.LimiteBusca(query.Limite)
        );

        var perfis = await perfilRepository.BuscarPerfisAsync(filtro, ct);

        return paginador.MontarPagina(
            perfis,
            query.Limite,
            perfil => new PerfilBuscaCursor(perfil.Nome, perfil.Id),
            BuscarPerfisResponse.FromModel
        );
    }
}
