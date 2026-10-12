namespace Instartups.Query.Domain.Enums.Mapear;

public static class TiposPerfisEnumMapExtension
{
    public static string Map(this TiposPerfisEnum tipoPerfil)
    {
        return tipoPerfil switch
        {
            TiposPerfisEnum.STARTUP => "STARTUP",
            TiposPerfisEnum.INVESTIDOR => "INVESTIDOR",
            _ => "ADMIN"
        };
    }
}
