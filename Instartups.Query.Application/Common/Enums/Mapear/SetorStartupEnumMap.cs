namespace Instartups.Query.Domain.Enums.Mapear;

public static class SetorStartupEnumMap
{
    public static string Map(this SetorStartupEnum setor)
    {
        return setor switch
        {
            SetorStartupEnum.Financas => "FINANCAS",
            SetorStartupEnum.Saude => "SAUDE",
            SetorStartupEnum.Agronegocios => "AGRONEGOCIOS",
            SetorStartupEnum.Varejo => "VAREJO",
            _ => "ERRO"
        };
    }

    public static SetorStartupEnum MapSetorStartup(this string setor)
    {
        return setor.Trim().ToUpperInvariant() switch
        {
            "FINANCAS" => SetorStartupEnum.Financas,
            "SAUDE" => SetorStartupEnum.Saude,
            "AGRONEGOCIOS" => SetorStartupEnum.Agronegocios,
            "VAREJO" => SetorStartupEnum.Varejo,
            _ => 0
        };
    }
}
