using Instartups.Query.Application.Common.Models;
using Instartups.Query.Application.UseCases.PegarFeed.Filtros;
using Instartups.Query.Application.UseCases.PegarPostagensPerfil.Filtros;

namespace Instartups.Query.Application.Interfaces.Repositories;

public interface IPostagemRepository
{
    public Task<IReadOnlyList<PostagemModel>> PegarFeedAsync(FiltroFeed filtro, CancellationToken ct);
    public Task<IReadOnlyList<PostagemModel>> PegarPostagensPerfilAsync(FiltroPostagensPerfil filtro, CancellationToken ct);
}
