using Instartups.Query.Application.UseCases.PegarFeed.Filtros;
using Instartups.Query.Application.UseCases.PegarFeed.Models;

namespace Instartups.Query.Application.Interfaces.Repositories;

public interface IPostagemRepository
{
    public Task<IReadOnlyList<PostagemFeedModel>> PegarFeedAsync(FiltroFeed filtro, CancellationToken ct);
}
