using Instartups.Query.Application.Common.Models;
using Instartups.Query.Domain.Enums;
using Instartups.Query.Domain.Enums.Mapear;

namespace Instartups.Query.Application.Common.Response;

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
