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
}
