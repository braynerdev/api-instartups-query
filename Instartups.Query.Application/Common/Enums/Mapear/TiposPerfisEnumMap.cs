namespace Instartups.Query.Domain.Enums.Mapear;

public static class TiposPerfisEnumMapExtension
{
    public static string Map(this TiposPerfisEnum tipoPerfil)
    {
        return tipoPerfil switch
        {
            TiposPerfisEnum.Startup => "STARTUP",
            TiposPerfisEnum.Investidor => "INVESTIDOR",
            _ => "ADMIN"
        };
    }

    public static TiposPerfisEnum MapTiposPerfis(this string tipo)
    {
        return tipo.Trim().ToUpperInvariant() switch
        {
            "STARTUP" => TiposPerfisEnum.Startup,
            "INVESTIDOR" => TiposPerfisEnum.Investidor,
            _ => 0
        };
    }
}
