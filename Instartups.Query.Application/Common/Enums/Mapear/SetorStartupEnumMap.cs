namespace Instartups.Query.Domain.Enums.Mapear;

public static class SetorStartupEnumMap
{
    private static readonly Dictionary<SetorStartupEnum, (string Codigo, string Rotulo)> Setores = new()
    {
        [SetorStartupEnum.Financas] = ("FINANCAS", "Finanças"),
        [SetorStartupEnum.Saude] = ("SAUDE", "Saúde"),
        [SetorStartupEnum.Agronegocios] = ("AGRONEGOCIOS", "Agronegócios"),
        [SetorStartupEnum.Varejo] = ("VAREJO", "Varejo")
    };

    private static readonly Dictionary<string, SetorStartupEnum> SetoresPorCodigo = Setores
        .ToDictionary(setor => setor.Value.Codigo, setor => setor.Key);

    public static string Map(this SetorStartupEnum setor)
    {
        return Setores.TryGetValue(setor, out var valores) ? valores.Codigo : "ERRO";
    }

    public static string Rotulo(this SetorStartupEnum setor)
    {
        return Setores.TryGetValue(setor, out var valores) ? valores.Rotulo : "ERRO";
    }

    public static SetorStartupEnum MapSetorStartup(this string setor)
    {
        return SetoresPorCodigo.TryGetValue(setor.Trim().ToUpperInvariant(), out var valor) ? valor : 0;
    }
}
