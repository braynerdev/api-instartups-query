using Instartups.Query.Application.UseCases.PegarFeed.Models;
using Instartups.Query.Domain.Enums;
using Instartups.Query.Domain.Enums.Mapear;

namespace Instartups.Query.Application.UseCases.PegarFeed.Responses;

public sealed record MidiaPostagemResponse(
    Guid Id,
    string Url,
    string Tipo
)
{
    public static MidiaPostagemResponse FromModel(MidiaPostagemModel midia)
    {
        return new MidiaPostagemResponse(
            midia.Id,
            midia.UrlMidia,
            ((TiposMidiaEnum)midia.TipoMidia).Map()
        );
    }
}
