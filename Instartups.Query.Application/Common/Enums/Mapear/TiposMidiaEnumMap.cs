namespace Instartups.Query.Domain.Enums.Mapear;

public static class TiposMidiaEnumMap
{
    public static string Map(this TiposMidiaEnum tipoMidiaEnum)
    {
        return tipoMidiaEnum switch
        {
            TiposMidiaEnum.Foto => "FOTO",
            TiposMidiaEnum.Video => "VIDEO",
            _ => "ERRO"
        };
    }

    public static TiposMidiaEnum MapTiposMidia(this string tipo)
    {
        return tipo.Trim().ToUpperInvariant() switch
        {
            "FOTO" => TiposMidiaEnum.Foto,
            "VIDEO" => TiposMidiaEnum.Video,
            _ => 0
        };
    }
}