using Instartups.Query.Domain.Exceptions;

namespace Instartups.Query.Domain.Enums.Mapear;

public static class TiposMidiaEnumMap
{
    public static string Map(this TiposMidiaEnum tipoMidiaEnum)
    {
        return tipoMidiaEnum switch
        {
            TiposMidiaEnum.FOTO => "FOTO",
            TiposMidiaEnum.VIDEO => "VÍDEO",
            _ => "ERRO"
        };
    }

    public static TiposMidiaEnum MapTiposMidia(this string tipo)
    {
        return tipo.Trim().ToUpperInvariant() switch
        {
            "FOTO" => TiposMidiaEnum.FOTO,
            "VIDEO" => TiposMidiaEnum.VIDEO,
            _ => 0
        };
    }
}